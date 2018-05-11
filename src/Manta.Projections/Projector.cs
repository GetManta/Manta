using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Manta.Projections
{
    public class Projector : ProjectorBase
    {
        public Projector(string name, IStreamDataSource dataSource, IProjectionCheckpointRepository checkpointRepository, int batchSize = 1000)
            : base(name, dataSource, checkpointRepository, batchSize) { }

        internal override async Task<List<DispatchingResult>> RunOnce(CancellationToken token)
        {
            var projectionDescriptors = GetActiveDescriptors();
            if (projectionDescriptors.Count == 0) return new List<DispatchingResult>(0);

            var projectionsFlow = PrepareProjectionsFlow(token);
            var flow = ProduceProjectionsFlow(projectionsFlow, projectionDescriptors, token);
            var consumer = ConsumeProjectionsFlow(projectionsFlow, token).NotOnCapturedContext();

            await Task.WhenAll(flow, projectionsFlow.Completion);

            var results = await consumer;
            Console.WriteLine("Projection groups " + results.Count);

            return results;
        }

        private async Task ProduceProjectionsFlow(TransformBlock<List<ProjectionDescriptor>, DispatchingResult> projectionsFlow, List<ProjectionDescriptor> projectionDescriptors, CancellationToken token)
        {
            var positionRanges = this.GenerateRanges(
                projectionDescriptors.Min(x => x.Checkpoint.Position),
                projectionDescriptors.Max(x => x.Checkpoint.Position),
                BatchSize);

            foreach (var projectionGroup in projectionDescriptors.GroupBy(x => positionRanges.FirstOrDefault(r => r > x.Checkpoint.Position)))
            {
                await projectionsFlow.SendAsync(projectionGroup.ToList(), token);
            }
            projectionsFlow.Complete();
        }

        private static async Task<List<DispatchingResult>> ConsumeProjectionsFlow(TransformBlock<List<ProjectionDescriptor>, DispatchingResult> projectionsFlow, CancellationToken token)
        {
            var results = new List<DispatchingResult>(2);
            while(await projectionsFlow.OutputAvailableAsync(token))
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
                async raw => await DeserializeTransformation(raw),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = 200
                });
        }

        private async Task<List<MessageEnvelope>> DeserializeEnvelopes(List<ProjectionDescriptor> descriptors, CancellationToken token)
        {
            var deserializeBlock = PrepareDeserializationFlow(token);

            var fromPosition = descriptors.Min(x => x.Checkpoint.Position);
            var fetcher = DataSource.Fetch(deserializeBlock, fromPosition, BatchSize, token);
            var consumer = ConsumeDeserializedEnvelopes(deserializeBlock, token).NotOnCapturedContext();

            await Task.WhenAll(fetcher, deserializeBlock.Completion);

            var envelopes = await consumer;
            Console.WriteLine("Received " + envelopes.Count);
            return envelopes;
        }

        private async Task<List<MessageEnvelope>> ConsumeDeserializedEnvelopes(TransformBlock<MessageRaw, MessageEnvelope> deserializeBlock, CancellationToken token)
        {
            var envelopes = new List<MessageEnvelope>(BatchSize / 4);
            while(await deserializeBlock.OutputAvailableAsync(token))
            {
                var e = await deserializeBlock.ReceiveAsync(token).NotOnCapturedContext();
                envelopes.Add(e);
            }

            return envelopes;
        }

        private Task<MessageEnvelope> DeserializeTransformation(MessageRaw raw)
        {
            try
            {
                var envelope = new MessageEnvelope
                {
                    Message = new object(),
                    Meta = new Metadata
                    {
                        CustomMetadata = null,
                        CorrelationId = raw.CorrelationId,
                        MessageContractName = raw.MessageContractName,
                        MessageId = raw.MessageId,
                        MessagePosition = raw.MessagePosition,
                        MessageVersion = raw.MessageVersion,
                        StreamId = raw.StreamId,
                        Timestamp = raw.Timestamp
                    }
                };

                Console.WriteLine("Deserializing " + raw.MessagePosition);

                //await Task.Delay(1);

                return Task.FromResult(envelope);
            }
            catch(Exception e)
            {
                // log exception
                return null;
            }
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
                        foreach (var envelope in envelopes)
                        {
                            if (envelope.Message == null) continue;
                            var dispatched = await Dispatch(descriptors, envelope).NotOnCapturedContext();
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
                // log exception
                return new DispatchingResult(e, 0, 0, false);
            }
        }

        private async Task<bool> Dispatch(IEnumerable<ProjectionDescriptor> projections, MessageEnvelope envelope)
        {
            var messageType = envelope.Message.GetType();
            var anyDispatched = false;
            foreach (var projection in projections)
            {
                if (projection.Checkpoint.DroppedAtUtc != null) continue;
                if (projection.Checkpoint.Position >= envelope.Meta.MessagePosition) continue;
                if (!projection.IsProjecting(messageType)) continue;

                var context = new ProjectingContext(MaxProjectingRetries);
                var dispatched = await DispatchProjection(projection, envelope, context);
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
                    await TryDispatch(projection, envelope, context);
                    return true;
                }
                catch (Exception ex)
                {
                    ProjectingError(projection, envelope, context, ex);
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
            var instance = ProjectionFactory.CreateProjectionInstance(projection.ProjectionType);
            if (instance == null) throw new NullReferenceException($"Projection instance {projection.ProjectionType.FullName} is null.");
            await ((dynamic)instance).On((dynamic)envelope.Message, envelope.Meta, context);
        }
    }
}