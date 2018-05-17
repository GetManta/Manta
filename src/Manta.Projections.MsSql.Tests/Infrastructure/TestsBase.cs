using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Manta.Projections.MsSql.Tests.Infrastructure
{
    [Collection("Manta collection")]
    public abstract class TestsBase : IDisposable
    {
        private readonly DatabaseInstance _dbInstance;

        protected TestsBase(LocalDbFixture fixture)
        {
            _dbInstance = new DatabaseInstance(fixture.LocalDb);
            ConnectionString = _dbInstance.ConnectionString;
        }

        protected string ConnectionString { get; }

        protected async Task<Projector> GetProjector(Action<ProjectorBase> cfg = null)
        {
            return await _dbInstance.GetProjector(cfg);
        }

        public void Dispose()
        {
            _dbInstance.Dispose();
        }
    }
}