using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Manta.Sceleton;
using Manta.Sceleton.Converters;
using Manta.Sceleton.Logging;

namespace Manta.Projections
{
    public abstract class ProjectorBase
    {
        private readonly IProjectionCheckpointRepository _checkpointRepository;
        private readonly List<ProjectionDescriptor> _projectionDescriptors;
        private Action<ProjectingException> _onProjectionError;
        private Action<ProjectorStats> _onProjectorExecute;

        protected ProjectorBase(string name, IStreamDataSource dataSource, IProjectionCheckpointRepository checkpointRepository, ISerializer serializer, int batchSize)
        {
            if (name.IsNullOrEmpty()) throw new ArgumentException("Name of projector must be set.", nameof(name));
            if (batchSize <= 0) throw new ArgumentException("Batch size must be greater than zero.", nameof(batchSize));

            _checkpointRepository = checkpointRepository ?? throw new ArgumentNullException(nameof(checkpointRepository));
            ProjectionFactory = new ActivatorProjectionFactory();
            Logger = new NullLogger();
            Name = name;
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            BatchSize = batchSize;
            MaxProjectingRetries = 3;

            _projectionDescriptors = new List<ProjectionDescriptor>(20);
        }

        public string Name { get; }
        public IStreamDataSource DataSource { get; }
        public ISerializer Serializer { get; }
        public IUpConverterFactory UpConverterFactory { get; private set; }
        public byte MaxProjectingRetries { get; }
        public int BatchSize { get; }
        internal ILogger Logger { get; private set; }

        public ProjectionDescriptor[] GetProjections()
        {
            return _projectionDescriptors.ToArray();
        }

        public ProjectorBase AddProjection<TProjection>() where TProjection : Projection
        {
            AddProjection(typeof(TProjection));
            return this;
        }

        public ProjectorBase AddProjection(Type projectionType)
        {
            if (!typeof(Projection).IsAssignableFrom(projectionType))
                throw new InvalidOperationException($"Type '{projectionType.FullName}' is not {typeof(Projection).Name} type.");

            if (_projectionDescriptors.Any(x => x.ProjectionType == projectionType)) return this;
            _projectionDescriptors.Add(new ProjectionDescriptor(projectionType));
            return this;
        }

        public ProjectorBase AddProjectionFactory(IProjectionFactory projectionFactory)
        {
            ProjectionFactory = projectionFactory ?? new ActivatorProjectionFactory();
            return this;
        }

        public ProjectorBase AddProjections(Assembly assembly, Func<Type, bool> filter = null)
        {
            var projections = assembly.GetTypes().Where(t => typeof(Projection).IsAssignableFrom(t) && (filter?.Invoke(t) ?? true));
            foreach (var type in projections)
            {
                AddProjection(type);
            }
            return this;
        }

        public ProjectorBase AddLogger(ILogger logger)
        {
            Logger = logger ?? new NullLogger();
            return this;
        }

        public ProjectorBase WithDefaultUpConverterFactory(params Assembly[] assemblies)
        {
            return WithUpConverterFactory(new DefaultUpConverterFactory(assemblies));
        }

        public ProjectorBase WithUpConverterFactory(IUpConverterFactory factory)
        {
            UpConverterFactory = factory;
            return this;
        }

        public ProjectorBase WithStatistics(Action<ProjectorStats> stats)
        {
            _onProjectorExecute = stats;
            return this;
        }

        public ProjectorBase OnProjectingError(Action<ProjectingException> onProjectionError)
        {
            _onProjectionError = onProjectionError;
            return this;
        }

        public async Task<DispatchingResult[]> Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            await InitializeCheckpoints(cancellationToken).NotOnCapturedContext();

            var stats = new List<DispatchingResult>();
            while (true)
            {
                var results = await RunOnce(cancellationToken).NotOnCapturedContext();
                stats.AddRange(results);

                var resultWithException = results.FirstOrDefault(x => x.HaveCaughtException());
                if (resultWithException != null)
                {
                    Logger.Error("Projection '{0}' dropped and returns exception: {1}", resultWithException.Descriptor.ContractName, resultWithException.Exception.Message);
                }

                if (results.All(x => x.AnyDispatched == false)) break;
            }

            _onProjectorExecute?.Invoke(new ProjectorStats(stats));

            return stats.ToArray();
        }

        internal abstract Task<List<DispatchingResult>> RunOnce(CancellationToken token);

        public async Task InitializeCheckpoints(CancellationToken token = default(CancellationToken))
        {
            var checkpoints = (await _checkpointRepository.Fetch(token).NotOnCapturedContext()).ToList();
            foreach (var descriptor in _projectionDescriptors)
            {
                descriptor.SetCheckpoint(checkpoints.FirstOrDefault(x => x.ProjectionName == descriptor.ContractName)
                    ?? await _checkpointRepository.AddCheckpoint(Name, descriptor.ContractName, token).NotOnCapturedContext());
            }

            await _checkpointRepository.Delete(
                checkpoints.Where(x => _projectionDescriptors.All(z => z.Checkpoint != x)).ToArray(),
                token).NotOnCapturedContext();
        }

        protected IProjectionFactory ProjectionFactory { get; private set; }
        protected List<ProjectionDescriptor> GetActiveDescriptors() => _projectionDescriptors.Where(x => x.Checkpoint.DroppedAtUtc == null).ToList();

        protected internal async Task UpdateCheckpoint(IProjectionCheckpoint checkpoint, bool undropRequested, CancellationToken token)
        {
            await _checkpointRepository.Update(checkpoint, undropRequested, token).NotOnCapturedContext();
        }

        protected void ProjectingError(ProjectingException exception)
        {
            _onProjectionError?.Invoke(exception);
        }
    }
}