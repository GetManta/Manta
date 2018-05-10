using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Manta.Projections.MsSql
{
    public class MsSqlCheckpointRepository : IProjectionCheckpointRepository
    {
        private readonly string _connectionString;

        public MsSqlCheckpointRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<IEnumerable<IProjectionCheckpoint>> Fetch(CancellationToken token = default(CancellationToken))
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                return Task.FromResult((IEnumerable<IProjectionCheckpoint>)new IProjectionCheckpoint[0]);
            }
        }

        public Task Update(IEnumerable<IProjectionCheckpoint> checkpoints, CancellationToken token = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task Delete(IEnumerable<IProjectionCheckpoint> checkpoints, CancellationToken token = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task<IProjectionCheckpoint> AddCheckpoint(string projectorName, string projectionName, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult((IProjectionCheckpoint)null);
        }
    }
}