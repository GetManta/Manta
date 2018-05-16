using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Manta.Sceleton;

namespace Manta.Projections.MsSql
{
    public class MsSqlCheckpointRepository : IProjectionCheckpointRepository
    {
        private const string spuFetchAllProjectionCheckpoints = "mantaFetchAllProjectionCheckpoints";
        private const string spuAddProjectionCheckpoint = "mantaAddProjectionCheckpoint";
        private const string spuDeleteProjectionCheckpoint = "mantaDeleteProjectionCheckpoint";
        private const string spuUpdateProjectionCheckpoint = "mantaUpdateProjectionCheckpoint";

        private const byte colIndexForProjectorName = 0;
        private const byte colIndexForProjectionName = 1;
        private const byte colIndexForPosition = 2;
        private const byte colIndexForDroppedAtUtc = 3;

        private readonly string _connectionString;

        public MsSqlCheckpointRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<IProjectionCheckpoint>> Fetch(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spuFetchAllProjectionCheckpoints;

                await cnn.OpenAsync(cancellationToken).NotOnCapturedContext();
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, cancellationToken).NotOnCapturedContext())
                {
                    var result = new List<IProjectionCheckpoint>(20);
                    while (await reader.ReadAsync(cancellationToken).NotOnCapturedContext())
                    {

                        result.Add(
                            new CheckpointState
                            {
                                ProjectorName = await reader.GetFieldValueAsync<string>(colIndexForProjectorName, cancellationToken).NotOnCapturedContext(),
                                ProjectionName = await reader.GetFieldValueAsync<string>(colIndexForProjectionName, cancellationToken).NotOnCapturedContext(),
                                Position = await reader.GetFieldValueAsync<long>(colIndexForPosition, cancellationToken).NotOnCapturedContext(),
                                DroppedAtUtc = await reader.IsDBNullAsync(colIndexForDroppedAtUtc, cancellationToken)
                                    ? null
                                    : await reader.GetFieldValueAsync<DateTime?>(colIndexForDroppedAtUtc, cancellationToken).NotOnCapturedContext()
                            });
                    }

                    return result;
                }
            }
        }

        public async Task Update(IEnumerable<IProjectionCheckpoint> checkpoints, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                await cnn.OpenAsync(cancellationToken).NotOnCapturedContext();
                foreach (var checkpoint in checkpoints)
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = spuUpdateProjectionCheckpoint;
                    cmd.AddInputParam("@ProjectorName", DbType.AnsiString, checkpoint.ProjectorName, 128);
                    cmd.AddInputParam("@ProjectionName", DbType.AnsiString, checkpoint.ProjectionName, 128);
                    cmd.AddInputParam("@Position", DbType.Int64, checkpoint.Position);
                    cmd.AddInputParam("@DroppedAtUtc", DbType.DateTime2, checkpoint.DroppedAtUtc ?? (object)DBNull.Value);
                    await cmd.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                }
            }
        }

        public async Task Delete(IEnumerable<IProjectionCheckpoint> checkpoints, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                await cnn.OpenAsync(cancellationToken).NotOnCapturedContext();
                foreach (var checkpoint in checkpoints)
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = spuDeleteProjectionCheckpoint;
                    cmd.AddInputParam("@ProjectorName", DbType.AnsiString, checkpoint.ProjectorName, 128);
                    cmd.AddInputParam("@ProjectionName", DbType.AnsiString, checkpoint.ProjectionName, 128);
                    await cmd.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                }
            }
        }

        public async Task<IProjectionCheckpoint> AddCheckpoint(string projectorName, string projectionName, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cnn = new SqlConnection(_connectionString))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spuAddProjectionCheckpoint;

                var checkpoint = new CheckpointState
                {
                    ProjectorName = projectorName,
                    ProjectionName = projectionName
                };

                cmd.AddInputParam("@ProjectorName", DbType.AnsiString, checkpoint.ProjectorName, 128);
                cmd.AddInputParam("@ProjectionName", DbType.AnsiString, checkpoint.ProjectionName, 128);

                await cnn.OpenAsync(cancellationToken).NotOnCapturedContext();
                await cmd.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                return checkpoint;
            }
        }
    }
}