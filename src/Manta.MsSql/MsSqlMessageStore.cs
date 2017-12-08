using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Manta.Sceleton;

namespace Manta.MsSql
{
    public class MsSqlMessageStore : IMessageStore
    {
        private readonly MsSqlMessageStoreSettings _settings;
        private readonly TransactionOptions _transactionOptions = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };

        public MsSqlMessageStore(MsSqlMessageStoreSettings settings)
        {
            _settings = settings;
        }

        public Task<RecordedStream> ReadStreamForward(string name, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public async Task AppendToStream(string name, int expectedVersion, UncommittedMessages data, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, _transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    switch (expectedVersion)
                    {
                        default:
                            await AppendToStreamWithExpectedVersion(name, expectedVersion, data, cancellationToken).NotOnCapturedContext();
                            break;
                        case ExpectedVersion.Any:
                            await AppendToStreamWithAnyVersion(name, data, cancellationToken).NotOnCapturedContext();
                            break;
                        case ExpectedVersion.NoStream:
                            await AppendToStreamWithNoStream(name, data, cancellationToken).NotOnCapturedContext();
                            break;
                    }
                    scope.Complete();
                }
                catch (SqlException e)
                {
                    if (e.IsUniqueConstraintViolation() || e.IsWrongExpectedVersionRised())
                    {
                        throw new WrongExpectedVersionException($"Saving new or existing stream '{name}' error. Stream exists.", e);
                    }
                    throw;
                }
            }
        }

        private Task AppendToStreamWithExpectedVersion(string name, int expectedVersion, UncommittedMessages data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task AppendToStreamWithAnyVersion(string name, UncommittedMessages data, CancellationToken cancellationToken)
        {
            if (data.Messages.Length > _settings.BatchSize)
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                {
                    using (var batch = new SqlClientSqlCommandSet(connection))
                    {
                        foreach (var msg in data.Messages)
                        {
                            var cmd = connection.CreateCommandToAppendingWithAnyVersion(name, data, msg);
                            batch.Append(cmd);
                        }
                        await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                        await batch.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                    }
                }
            }
            else
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                    foreach (var msg in data.Messages)
                    {
                        using (var cmd = connection.CreateCommandToAppendingWithAnyVersion(name, data, msg))
                        {
                            await cmd.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                        }
                    }
                }
            }
        }

        private Task AppendToStreamWithNoStream(string name, UncommittedMessages data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IMessageStoreAdvanced Advanced { get; }
    }
}
