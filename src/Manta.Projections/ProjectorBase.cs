using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Manta.Sceleton;
using Manta.Sceleton.Logging;

namespace Manta.Projections
{
    public abstract class ProjectorBase
    {
        private readonly IProjectionCheckpointRepository _checkpointRepository;
        private readonly List<ProjectionDescriptor> _projectionDescriptors;
        private Action<ProjectionDescriptor, MessageEnvelope, ProjectingContext, Exception> _onProjectionError;

        protected ProjectorBase(string name, IStreamDataSource dataSource, IProjectionCheckpointRepository checkpointRepository, int batchSize)
        {
            _checkpointRepository = checkpointRepository;
            ProjectionFactory = new ActivatorProjectionFactory();
            Logger = new NullLogger();
            Name = name;
            DataSource = dataSource;
            BatchSize = batchSize;
            MaxProjectingRetries = 3;

            _projectionDescriptors = new List<ProjectionDescriptor>(20);
        }

        public string Name { get; }
        public IStreamDataSource DataSource { get; }
        public byte MaxProjectingRetries { get; }
        public int BatchSize { get; }

        public void AddProjection<TProjection>() where TProjection : Projection
        {
            AddProjection(typeof(TProjection));
        }

        public void AddProjection(Type projectionType)
        {
            if (!typeof(Projection).IsAssignableFrom(projectionType))
                throw new InvalidOperationException($"Type '{projectionType.FullName}' is not {typeof(Projection).Name} type.");

            if (_projectionDescriptors.Any(x => x.ProjectionType == projectionType)) return;
            _projectionDescriptors.Add(new ProjectionDescriptor(projectionType));
        }

        public void AddProjectionFactory(IProjectionFactory projectionFactory)
        {
            ProjectionFactory = projectionFactory ?? new ActivatorProjectionFactory();
        }

        public void AddProjections(Assembly assembly, Func<Type, bool> filter = null)
        {
            var projections = assembly.GetTypes().Where(t => typeof(Projection).IsAssignableFrom(t) && (filter?.Invoke(t) ?? true));
            foreach (var type in projections.Where(filter))
            {
                AddProjection(type);
            }
        }

        public void AddLogger(ILogger logger)
        {
            Logger = logger ?? new NullLogger();
        }

        public void OnProjectingError(Action<ProjectionDescriptor, MessageEnvelope, ProjectingContext, Exception> onProjectionError)
        {
            _onProjectionError = onProjectionError;
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public async Task Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            await PrepareCheckpoints(cancellationToken).NotOnCapturedContext();

            //while (true)
            {
                var results = await RunOnce(cancellationToken).NotOnCapturedContext();
                PrintStats(results);
                //if (results.All(x => x.AnyDispatched == false)) break;
            }
        }

        private static void PrintStats(List<DispatchingResult> results)
        {
            var totalTime = (double)results.Sum(x => x.ElapsedMilliseconds) / 1000;
            var totalMessages = results.Sum(x => x.EnvelopesCount);
            var avg = Math.Round(totalMessages / totalTime, 2, MidpointRounding.AwayFromZero);
            Console.WriteLine($"Total time {totalTime}sec | Average processing {avg}/sec.");
        }

        internal abstract Task<List<DispatchingResult>> RunOnce(CancellationToken cancellationToken);

        private async Task PrepareCheckpoints(CancellationToken cancellationToken)
        {
            var checkpoints = (await _checkpointRepository.Fetch(cancellationToken).NotOnCapturedContext()).ToList();
            foreach (var descriptor in _projectionDescriptors)
            {
                descriptor.Checkpoint = checkpoints.FirstOrDefault(x => x.ProjectionName == descriptor.ContractName)
                    ?? await _checkpointRepository.AddCheckpoint(Name, descriptor.ContractName, cancellationToken).NotOnCapturedContext();
            }

            await _checkpointRepository.Delete(
                checkpoints.Where(x => _projectionDescriptors.All(z => z.Checkpoint != x)),
                cancellationToken).NotOnCapturedContext();
        }

        protected ILogger Logger { get; private set; }
        protected IProjectionFactory ProjectionFactory { get; private set; }
        protected List<ProjectionDescriptor> GetActiveDescriptors() => _projectionDescriptors.Where(x => x.Checkpoint.DroppedAtUtc == null).ToList();

        protected async Task UpdateDescriptors(IEnumerable<ProjectionDescriptor> descriptors, CancellationToken cancellationToken)
        {
            await _checkpointRepository.Update(descriptors.Select(x => x.Checkpoint).ToList(), cancellationToken).NotOnCapturedContext();
        }

        protected void ProjectingError(ProjectionDescriptor projection, MessageEnvelope envelope, ProjectingContext context, Exception exception)
        {
            _onProjectionError?.Invoke(projection, envelope, context, exception);
        }

    }
}