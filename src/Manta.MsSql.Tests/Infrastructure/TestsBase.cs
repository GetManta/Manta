using System;
using System.Threading.Tasks;
using Xunit;

namespace Manta.MsSql.Tests.Infrastructure
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

        protected async Task<IMessageStore> GetMessageStore()
        {
            return await _dbInstance.GetMessageStore();
        }

        public void Dispose()
        {
            _dbInstance.Dispose();
        }
    }
}