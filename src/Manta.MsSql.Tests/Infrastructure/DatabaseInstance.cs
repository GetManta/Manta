using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Manta.MsSql.Installer;
using Manta.Sceleton;

namespace Manta.MsSql.Tests.Infrastructure
{
    public class DatabaseInstance : IDisposable
    {
        private const string connectionStringTemplate = "data source=(local); initial catalog = [database-name]; Integrated Security = True; MultipleActiveResultSets = true";
        private const string databaseNameToken = "[database-name]";

        private readonly string _masterConnectionString;
        private readonly string _databaseName;
        private bool _databaseCreated;

        public DatabaseInstance()
        {
            _databaseName = "MantaTests_" + Guid.NewGuid().ToString("N");
            ConnectionString = connectionStringTemplate.Replace(databaseNameToken, _databaseName);
            _masterConnectionString = connectionStringTemplate.Replace(databaseNameToken, "master");
        }

        public string ConnectionString { get; }

        public async Task<IMessageStore> GetMessageStore(bool batching = true)
        {
            if (!_databaseCreated) await CreateDatabase().NotOnCapturedContext();
            await ClearDatabase();
            return new MsSqlMessageStore(new MsSqlMessageStoreSettings(ConnectionString, batching));
        }

        private async Task ClearDatabase()
        {
            Debug.WriteLine($"Clearing database {_databaseName}");
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync().NotOnCapturedContext();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"TRUNCATE TABLE [dbo].[MantaStreams];UPDATE [dbo].[MantaStreamsStats] SET MaxMessagePosition = 0, CountOfAllMessages = 0;";
                    await cmd.ExecuteNonQueryAsync().NotOnCapturedContext();
                }
            }
        }

        private async Task CreateDatabase()
        {
            Debug.WriteLine($"Creating database {_databaseName}");
            var commandText = $"CREATE DATABASE {_databaseName}";

            using (var connection = new SqlConnection(_masterConnectionString))
            {
                await connection.OpenAsync().NotOnCapturedContext();
                using (var command = new SqlCommand(commandText, connection))
                {
                    await command.ExecuteNonQueryAsync().NotOnCapturedContext();
                }
            }

            var installer = new MsSqlMessageStoreInstaller(ConnectionString);
            await installer.Execute();

            _databaseCreated = true;
        }

        public void Dispose()
        {
            DropDatabase();
        }

        public void DropDatabase()
        {
            if (!_databaseCreated) return;

            Debug.WriteLine($"Dropping database {_databaseName}");

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                // Fixes: "Cannot drop database because it is currently in use"
                SqlConnection.ClearPool(sqlConnection);
            }

            try
            {
                using (var cnn = new SqlConnection(_masterConnectionString))
                {
                    cnn.Open();
                    using (var command = new SqlCommand($"DROP DATABASE {_databaseName}", cnn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Nothing happened
            }
            finally
            {
                _databaseCreated = false;
            }
        }
    }
}