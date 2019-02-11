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
        public const int DefaultBatchSize = 500;
        private const int defaultCommandTimeoutInSeconds = 120;
        private const string paramBatchSize = "@BatchSize";
        private const string mantaLinearizeStreamsCommand = "mantaLinearizeStreams";

        private readonly string _connectionString;

        public MsSqlLinearizer(string connectionString, ILogger logger, int batchSize = DefaultBatchSize)
            : this(connectionString, logger, TimeSpan.Zero, TimeSpan.Zero, batchSize) { }

        public MsSqlLinearizer(string connectionString, ILogger logger, TimeSpan timeout, TimeSpan workDuration, int batchSize = DefaultBatchSize)
            : base(logger, timeout, workDuration)
        {
            if (connectionString.IsNullOrEmpty()) throw new ArgumentException("ConnectionString can not be null or empty.", nameof(connectionString));

            _connectionString = PrepareConnectionString(connectionString);
            BatchSize = batchSize;
        }

        /// <summary>
        /// Returns batch size for single execution of linearizing.
        /// </summary>
        public int BatchSize { get; }

        private static string PrepareConnectionString(string connectionString)
        {
            if (connectionString.ToLowerInvariant().Contains("enlist")) return connectionString;
            return connectionString.TrimEnd(';') + "; Enlist = false;"; // don't want to enlist
        }

        protected override async Task<bool> Linearize(CancellationToken cancellationToken)
        {
            if (_connectionString == null) return false;

            using (var scope = TransactionScopeHelper.New())
            {
                using (var cnn = new SqlConnection(_connectionString))
                using (var cmd = cnn.CreateCommand(mantaLinearizeStreamsCommand, defaultCommandTimeoutInSeconds))
                {
                    cmd.AddInputParam(paramBatchSize, SqlDbType.Int, BatchSize);

                    await cnn.OpenAsync(cancellationToken).NotOnCapturedContext();
                    var result = await cmd.ExecuteScalarAsync(cancellationToken)
                        .NotOnCapturedContext();

                    scope.Complete();

                    return result != null && result != DBNull.Value && (bool)result;
                }
            }
        }
    }
}