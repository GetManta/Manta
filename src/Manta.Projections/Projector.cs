using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Transactions;

namespace Manta.Projections
{
    public class Projector : ProjectorBase
    {
        public Projector(string name, IStreamDataSource dataSource, IProjectionCheckpointRepository checkpointRepository, int batchSize = 1000)
            : base(name, dataSource, checkpointRepository, batchSize) { }

        protected override async Task RunOnce(CancellationToken cancellationToken)
        {
            var projectionDescriptors = GetDescriptors();

            var projectionsFlow = PrepareProjectionsFlow(cancellationToken);

            var positionRanges = this.GenerateRanges(
                projectionDescriptors.Min(x => x.Checkpoint.Position),
                projectionDescriptors.Max(x => x.Checkpoint.Position),
                BatchSize);

            foreach (var projectionGroup in projectionDescriptors.GroupBy(x => positionRanges.FirstOrDefault(r => r > x.Checkpoint.Position)))
            {
                await projectionsFlow.SendAsync(projectionGroup.Select(x => x).ToList(), cancellationToken);
            }

            await projectionsFlow.Completion;

            // podziel MessagePosition projekcji na bloki po 1000
            // dodaj do listy blokowe przetwarzanie dla wszystkich projekcji potrzebujących mniej zdarzeń niż 1000
            // dodaj do listy blokowe przetwarzanie dla każdej z projekcji potrzebujących więcej zdarzeń niż 1000
            // uruchom transformacje commitując/batch'ując per blok
        }

        private ActionBlock<List<ProjectionDescriptor>> PrepareProjectionsFlow(CancellationToken cancellationToken)
        {
            return new ActionBlock<List<ProjectionDescriptor>>(
                async descriptors => await DispatchProjectionsRange(descriptors, cancellationToken),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = Environment.ProcessorCount
                });
        }

        private TransformBlock<MessageRaw, MessageEnvelope> PrepareDeserializationFlow(CancellationToken cancellationToken)
        {
            return new TransformBlock<MessageRaw, MessageEnvelope>(
                raw => DeserializeTransformation(raw),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = BatchSize / 10,
                    EnsureOrdered = true
                });
        }

        private async Task DispatchProjectionsRange(List<ProjectionDescriptor> descriptors, CancellationToken cancellationToken)
        {
            var deserializeBlock = PrepareDeserializationFlow(cancellationToken);

            var fromPosition = descriptors.Min(x => x.Checkpoint.Position);
            await DataSource.Fetch(deserializeBlock, fromPosition, BatchSize, cancellationToken);

            var envelopes = new List<MessageEnvelope>(BatchSize / 10);
            for (var i = 0; i < BatchSize / 10; i++)
            {
                try
                {
                    envelopes.Add(await deserializeBlock.ReceiveAsync(cancellationToken));
                }
                catch
                {
                    break;
                }
            }

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    foreach (var envelope in envelopes)
                    {
                        await Dispatch(descriptors, envelope);
                    }


                    await UpdateDescriptors(descriptors, cancellationToken);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    // wystąpił nieobsłużony wyjątek w batchu

                    Logger.Error(e.ToString());
                }
            }
        }

        private MessageEnvelope DeserializeTransformation(MessageRaw raw)
        {
            var envelope = new MessageEnvelope
            {
                Event = null,
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

            return envelope;
        }

        private async Task Dispatch(IEnumerable<ProjectionDescriptor> projections, MessageEnvelope envelope)
        {
            foreach (var projection in projections)
            {
                if (projection.Checkpoint.Position >= envelope.Meta.MessagePosition) return;
                await DispatchProjection(projection, envelope);
            }
        }

        private Task DispatchProjection(ProjectionDescriptor projectionDescriptor, MessageEnvelope envelope)
        {
            // execute handler on projection
            // try 3 times
            // check return status from ProjectingContext
            // if 3 times throws then mark that projection as abandoned
            // otherwise assign last processed position to envelope.MessagePosition
            return Task.CompletedTask;
        }
    }
}