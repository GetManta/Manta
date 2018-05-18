using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Manta.Sceleton;

namespace Manta.Projections
{
    public class Projector : ProjectorBase
    {
        public Projector(string name, IStreamDataSource dataSource, IProjectionCheckpointRepository checkpointRepository, ISerializer serializer, int batchSize = 1000)
            : base(name, dataSource, checkpointRepository, serializer, batchSize) { }

        internal override async Task<List<DispatchingResult>> RunOnce(CancellationToken token)
        {
            var projectionDescriptors = GetActiveDescriptors();
            if (projectionDescriptors.Count == 0) return new List<DispatchingResult>(0);

            var projectionsFlow = PrepareProjectionsFlow(token);
            var flow = ProduceProjectionsFlow(projectionsFlow, projectionDescriptors, token);
            var consumer = ConsumeProjectionsFlow(projectionsFlow, token).NotOnCapturedContext();

            await Task.WhenAll(flow, projectionsFlow.Completion);

            return await consumer;
        }

        private async Task ProduceProjectionsFlow(TransformBlock<List<ProjectionDescriptor>, DispatchingResult> projectionsFlow, List<ProjectionDescriptor> projectionDescriptors, CancellationToken token)
        {
            var positionRanges = this.GenerateRanges(
                projectionDescriptors.Min(x => x.Checkpoint.Position),
                projectionDescriptors.Max(x => x.Checkpoint.Position),
                BatchSize);

            foreach (var projectionGroup in projectionDescriptors.GroupBy(x => positionRanges.FirstOrDefault(r => r >= x.Checkpoint.Position)))
            {
                Logger.Debug("Calculated min position {0}.", projectionGroup.Key);
                await projectionsFlow.SendAsync(projectionGroup.ToList(), token).NotOnCapturedContext();
            }
            projectionsFlow.Complete();
        }

        private static async Task<List<DispatchingResult>> ConsumeProjectionsFlow(TransformBlock<List<ProjectionDescriptor>, DispatchingResult> projectionsFlow, CancellationToken token)
        {
            var results = new List<DispatchingResult>(2);
            while(await projectionsFlow.OutputAvailableAsync(token).NotOnCapturedContext())
            {
                var r = await projectionsFlow.ReceiveAsync(token).NotOnCapturedContext();
                results.Add(r);
            }

            return results;
        }

        private TransformBlock<List<ProjectionDescriptor>, DispatchingResult> PrepareProjectionsFlow(CancellationToken token)
        {
            return new TransformBlock<List<ProjectionDescriptor>, DispatchingResult>(
                async descriptors => await DispatchProjectionsRange(descriptors, token),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = Environment.ProcessorCount
                });
        }

        private TransformBlock<MessageRaw, MessageEnvelope> PrepareDeserializationFlow(CancellationToken token)
        {
            return new TransformBlock<MessageRaw, MessageEnvelope>(
                raw => DeserializeTransformation(raw),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = 250
                });
        }

        private async Task<List<MessageEnvelope>> DeserializeEnvelopes(List<ProjectionDescriptor> descriptors, CancellationToken token)
        {
            var deserializationFlow = PrepareDeserializationFlow(token);

            var fromPosition = descriptors.Min(x => x.Checkpoint.Position);
            var fetcher = DataSource.Fetch(deserializationFlow, fromPosition, BatchSize, token);
            var consumer = ConsumeDeserializedEnvelopes(deserializationFlow, token).NotOnCapturedContext();

            await Task.WhenAll(fetcher, deserializationFlow.Completion);
            
            var envelopes = await consumer;
            Logger.Debug("Deserialized {0} messages.", envelopes.Count);
            return envelopes;
        }

        private async Task<List<MessageEnvelope>> ConsumeDeserializedEnvelopes(TransformBlock<MessageRaw, MessageEnvelope> deserializeBlock, CancellationToken token)
        {
            var capacity = deserializeBlock.InputCount + deserializeBlock.OutputCount;
            if (capacity == 0) capacity = BatchSize / 4;

            var envelopes = new List<MessageEnvelope>(capacity);
            while(await deserializeBlock.OutputAvailableAsync(token).NotOnCapturedContext())
            {
                var e = await deserializeBlock.ReceiveAsync(token).NotOnCapturedContext();
                envelopes.Add(e);
            }

            return envelopes;
        }

        private MessageEnvelope DeserializeTransformation(MessageRaw raw)
        {
            var message = Serializer.DeserializeMessage(raw.MessageContractName, raw.MessagePayload);

            if (UpConverterFactory != null)
            {
                message = UpConvert(message.GetType(), message);
            }

            var metadata = new Metadata
            {
                CustomMetadata = Serializer.DeserializeMetadata(raw.MessageMetadataPayload),
                CorrelationId = raw.CorrelationId,
                MessageContractName = raw.MessageContractName,
                MessageId = raw.MessageId,
                MessagePosition = raw.MessagePosition,
                MessageVersion = raw.MessageVersion,
                StreamId = raw.StreamId,
                Timestamp = raw.Timestamp
            };

            return new MessageEnvelope(metadata, message);
        }

        private object UpConvert(Type messageType, object message)
        {
            var upConverter = UpConverterFactory.CreateInstanceFor(messageType);
            while (upConverter != null)
            {
                message = ((dynamic)upConverter).Convert((dynamic)message);
                upConverter = UpConverterFactory.CreateInstanceFor(message.GetType());
            }
            return message;
        }

        private async Task<DispatchingResult> DispatchProjectionsRange(List<ProjectionDescriptor> descriptors, CancellationToken token)
        {
            try
            {
                var envelopes = await DeserializeEnvelopes(descriptors, token).NotOnCapturedContext();
                var anyDispatched = false;
                
                var sw = new Stopwatch();
                sw.Start();

                using (var scope = this.NewTransactionScope())
                {
                    try
                    {
                        var context = new ProjectingContext(MaxProjectingRetries);
                        foreach (var envelope in envelopes)
                        {
                            if (envelope.Message == null) continue;
                            var dispatched = await Dispatch(descriptors, envelope, context).NotOnCapturedContext();
                            anyDispatched |= dispatched;
                        }

                        await UpdateDescriptors(descriptors, token).NotOnCapturedContext();
                        scope.Complete();
                    }
                    catch (Exception e) // unhandled exception in batch
                    {
                        sw.Stop();
                        return new DispatchingResult(e, envelopes.Count, sw.ElapsedMilliseconds, anyDispatched);
                    }
                }

                sw.Stop();
                return new DispatchingResult(envelopes.Count, sw.ElapsedMilliseconds, anyDispatched);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return new DispatchingResult(e, 0, 0, false);
            }
        }

        private async Task<bool> Dispatch(IEnumerable<ProjectionDescriptor> projections, MessageEnvelope envelope, ProjectingContext context)
        {
            var messageType = envelope.Message.GetType();

            var anyDispatched = false;
            foreach (var projection in projections)
            {
                if (projection.Checkpoint.DroppedAtUtc != null) continue;
                if (projection.Checkpoint.Position >= envelope.Meta.MessagePosition) continue;
                if (!projection.IsProjecting(messageType)) continue;

                context.Reset();
                var dispatched = await DispatchProjection(projection, envelope, context).NotOnCapturedContext();
                if (dispatched)
                {
                    projection.Checkpoint.Position = envelope.Meta.MessagePosition;
                    anyDispatched = true;
                }
            }

            return anyDispatched;
        }

        private async Task<bool> DispatchProjection(ProjectionDescriptor projection, MessageEnvelope envelope, ProjectingContext context)
        {
            while (true)
            {
                try
                {
                    await TryDispatch(projection, envelope, context).NotOnCapturedContext();
                    return true;
                }
                catch (ProjectingException ex)
                {
                    ProjectingError(ex);
                    if (context.ExceptionSolution == ExceptionSolutions.Ignore)
                    {
                        return true;
                    }
                    if (context.ExceptionSolution == ExceptionSolutions.Drop)
                    {
                        projection.Drop();
                        return false;
                    }
                    if (!context.CanRetry())
                    {
                        projection.Drop();
                        return false;
                    }
                    context.NextRetry();
                }
            }
        }

        private async Task TryDispatch(ProjectionDescriptor projection, MessageEnvelope envelope, ProjectingContext context)
        {
            try
            {
                var instance = ProjectionFactory.CreateProjectionInstance(projection.ProjectionType);
                if (instance == null) throw new NullReferenceException($"Projection instance {projection.ProjectionType.FullName} is null.");
                await ((dynamic)instance).On((dynamic)envelope.Message, envelope.Meta, context);
            }
            catch(Exception e)
            {
                throw new ProjectingException(projection, envelope, context, e);
            }
        }
    }

    /*
     * Group by message position range
     * - Fetch
     * - Deserialize
     * - Dispatch batch
     */
}