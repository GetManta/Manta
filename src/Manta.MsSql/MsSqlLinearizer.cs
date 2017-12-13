using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Manta.Sceleton;

namespace Manta.MsSql
{
    public class MsSqlLinearizer : Linearizer
    {
        private const int defaultCommandTimeout = 120000;
        private const string paramBatchSize = "@BatchSize";
        private const string mantaLinearizeStreamsCommand = "mantaLinearizeStreams";

        private readonly string _connectionString;

        public MsSqlLinearizer(ILogger logger, TimeSpan timeout, TimeSpan workDuration, string connectionString, int batchSize = 5000)
            : base(logger, timeout, workDuration)
        {
            if (connectionString.IsNullOrEmpty()) throw new ArgumentNullException(nameof(connectionString));

            _connectionString = PrepareConnectionString(connectionString);
            BatchSize = batchSize;
        }

        public int BatchSize { get; }

        private static string PrepareConnectionString(string connectionString)
        {
            if (connectionString.ToLowerInvariant().Contains("enlist")) return connectionString;
            return connectionString.TrimEnd(';') + "; Enlist = false;";
        }

        protected override async Task<bool> Linearize(CancellationToken cancellationToken)
        {
            if (_connectionString == null) return false;

            using (var cnn = new SqlConnection(_connectionString))
            using (var cmd = cnn.CreateCommand())
            {
                cmd.CommandText = mantaLinearizeStreamsCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = defaultCommandTimeout;
                var p = cmd.Parameters.Add(paramBatchSize, SqlDbType.Int);
                p.Value = BatchSize;

                await cnn.OpenAsync(cancellationToken).NotOnCapturedContext();
                var result = await cmd.ExecuteScalarAsync(cancellationToken)
                    .NotOnCapturedContext();

                return result != DBNull.Value && (bool)result; // null should never happen
            }
        }
    }
}