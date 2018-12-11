using System;
using System.Threading.Tasks;

namespace Manta.Projections.MsSql.Tests.Infrastructure
{
    public abstract class TestsBase : IDisposable
    {
        private readonly DatabaseInstance _dbInstance;

        protected TestsBase()
        {
            _dbInstance = new DatabaseInstance();
            ConnectionString = _dbInstance.ConnectionString;
        }

        protected string ConnectionString { get; }

        protected async Task<Projector> GetProjector(Action<ProjectorBase> cfg = null)
        {
            return await _dbInstance.GetProjector(cfg);
        }

        protected async Task<IMessageStore> GetMessageStore(bool batching = true)
        {
            return await _dbInstance.GetMessageStore(batching);
        }

        public void Dispose()
        {
            _dbInstance.Dispose();
        }
    }
}