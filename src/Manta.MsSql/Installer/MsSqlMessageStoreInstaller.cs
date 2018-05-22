using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Manta.Installer;
using Manta.Sceleton;
using Manta.Sceleton.Logging;

namespace Manta.MsSql.Installer
{
    public class MsSqlMessageStoreInstaller : MessageStoreInstaller
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public MsSqlMessageStoreInstaller(string connectionString, ILogger logger = null)
        {
            _connectionString = connectionString;
            _logger = logger ?? new NullLogger();
        }

        protected override async Task Install(Version installedVersion, CancellationToken token = default(CancellationToken))
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(token).NotOnCapturedContext();
                var tran = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    var scripts = SqlScripts.Scripts.GetScriptsFrom(installedVersion);
                    foreach (var script in scripts)
                    {
                        foreach (var query in script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.Transaction = tran;
                                cmd.CommandText = query.Trim();
                                if (string.IsNullOrEmpty(cmd.CommandText)) continue;
                                await cmd.ExecuteNonQueryAsync(token).NotOnCapturedContext();
                            }
                        }
                    }

                    await SetVersion(connection, tran, GetModuleVersion(), token).NotOnCapturedContext();

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        protected override async Task<Version> GetInstalledVersion(CancellationToken token = default(CancellationToken))
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommandForGetVersion())
            {
                await connection.OpenAsync(token).NotOnCapturedContext();
                var version = await cmd.ExecuteScalarAsync(token).NotOnCapturedContext();
                if (version == null || version == DBNull.Value) return null;
                _logger.Trace("Read version as {0}.", version);
                return Version.Parse((string)version);
            }
        }

        private async Task SetVersion(SqlConnection connection, SqlTransaction transaction, Version version, CancellationToken token = default(CancellationToken))
        {
            _logger.Trace("Set version as {0}.", version);

            using (var cmd = connection.CreateCommandForSetVersion(version))
            {
                cmd.Transaction = transaction;
                await cmd.ExecuteNonQueryAsync(token).NotOnCapturedContext();
            }
        }
    }
}