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

            var projectionsFlow = PrepareProjectionsRangeFlow(token);
            var producer = ProduceProjectionsFlow(projectionsFlow, projectionDescriptors, token);
            var consumer = ConsumeProjectionsFlow(projectionsFlow, projectionDescriptors.Count, token).NotOnCapturedContext();

            await Task.WhenAll(producer, projectionsFlow.Completion);

            return await consumer;
        }

        private async Task ProduceProjectionsFlow(TransformBlock<List<ProjectionDescriptor>, List<DispatchingResult>> flow, List<ProjectionDescriptor> descriptors, CancellationToken token)
        {
            var positionRanges = this.GenerateRanges(
                descriptors.Min(x => x.Checkpoint.Position),
                descriptors.Max(x => x.Checkpoint.Position),
                BatchSize);

            foreach (var projectionGroup in descriptors.GroupBy(x => positionRanges.FirstOrDefault(r => r >= x.Checkpoint.Position)))
            {
                Logger.Debug("Calculated min position {0}.", projectionGroup.Key);
                await flow.SendAsync(projectionGroup.ToList(), token).NotOnCapturedContext();
            }
            flow.Complete();
        }

        private static async Task<List<DispatchingResult>> ConsumeProjectionsFlow(TransformBlock<List<ProjectionDescriptor>, List<DispatchingResult>> flow, int activeDescriptors, CancellationToken token)
        {
            var results = new List<DispatchingResult>(activeDescriptors);
            while(await flow.OutputAvailableAsync(token).NotOnCapturedContext())
            {
                var r = await flow.ReceiveAsync(token).NotOnCapturedContext();
                results.AddRange(r);
            }

            return results;
        }

        private TransformBlock<List<ProjectionDescriptor>, List<DispatchingResult>> PrepareProjectionsRangeFlow(CancellationToken token)
        {
            return new TransformBlock<List<ProjectionDescriptor>, List<DispatchingResult>>(
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
                raw => DeserializationFlow.Transform(Serializer, UpConverterFactory, raw),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = 250
                });
        }

        private TransformBlock<DispatchingContext, DispatchingResult> PrepareProjectionDispathersFlow(CancellationToken token)
        {
            return new TransformBlock<DispatchingContext, DispatchingResult>(
                ctx => Dispatch(ctx.Descriptor, ctx.Envelopes, token),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = Environment.ProcessorCount
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

        private async Task<List<DispatchingResult>> DispatchProjectionsRange(List<ProjectionDescriptor> descriptors, CancellationToken token)
        {
            var envelopes = await DeserializeEnvelopes(descriptors, token).NotOnCapturedContext();

            var flow = PrepareProjectionDispathersFlow(token);
            var producer = ProduceProjectionDispatchersFlow(flow, descriptors, envelopes, token);
            var consumer = ConsumeProjectionDispatchersFlow(flow, token).NotOnCapturedContext();

            await Task.WhenAll(producer, flow.Completion);

            return await consumer;
        }

        private async Task<List<DispatchingResult>> ConsumeProjectionDispatchersFlow(TransformBlock<DispatchingContext, DispatchingResult> flow, CancellationToken token)
        {
            var capacity = flow.InputCount + flow.OutputCount;
            if (capacity == 0) capacity = BatchSize / 4;

            var results = new List<DispatchingResult>(capacity);
            while (await flow.OutputAvailableAsync(token).NotOnCapturedContext())
            {
                var e = await flow.ReceiveAsync(token).NotOnCapturedContext();
                results.Add(e);
            }

            return results;
        }

        private static async Task ProduceProjectionDispatchersFlow(TransformBlock<DispatchingContext, DispatchingResult> flow, List<ProjectionDescriptor> descriptors, List<MessageEnvelope> envelopes, CancellationToken token)
        {
            foreach (var descriptor in descriptors)
            {
                await flow.SendAsync(new DispatchingContext(descriptor, envelopes), token).NotOnCapturedContext();
            }
            flow.Complete();
        }


        private async Task<DispatchingResult> Dispatch(ProjectionDescriptor descriptor, List<MessageEnvelope> envelopes, CancellationToken token)
        {
            if (descriptor.IsDropped()) return DispatchingResult.StillDropped(descriptor);

            var sw = new Stopwatch();
            var context = new ProjectingContext(MaxProjectingRetries, descriptor.Checkpoint.Position);
            var anyDispatched = false;
            try
            {
                sw.Start();
                using (var scope = this.NewTransactionScope())
                {
                    foreach (var envelope in envelopes)
                    {
                        if (descriptor.Checkpoint.Position >= envelope.Meta.MessagePosition) continue;
                        if (!descriptor.IsProjecting(envelope.Message.GetType())) continue;

                        context.Reset();
                        if (await DispatchProjection(descriptor, envelope, context).NotOnCapturedContext())
                        {
                            descriptor.Checkpoint.Position = envelope.Meta.MessagePosition;
                            anyDispatched = true;
                        }
                    }

                    await UpdateCheckpoint(descriptor.Checkpoint, token).NotOnCapturedContext();
                    scope.Complete();
                }
                sw.Stop();
                return DispatchingResult.Dispatched(descriptor, envelopes.Count, sw.ElapsedMilliseconds, anyDispatched);
            }
            catch (Exception e) // error in batch
            {
                sw.Stop();
                descriptor.Checkpoint.Position = context.StartingBatchAtPosition; // restoring position
                await UpdateCheckpoint(descriptor.Checkpoint, token).NotOnCapturedContext();
                return DispatchingResult.DroppedOnException(descriptor, envelopes.Count, sw.ElapsedMilliseconds, e);
            }
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
                        throw;
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
            catch (Exception e)
            {
                throw new ProjectingException(projection, envelope, context, e);
            }
        }
    }
}