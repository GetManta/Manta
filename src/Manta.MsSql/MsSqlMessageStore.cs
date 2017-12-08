using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Manta.Sceleton;

namespace Manta.MsSql
{
    public class MsSqlMessageStore : IMessageStore
    {
        private readonly MsSqlMessageStoreSettings _settings;

        public MsSqlMessageStore(MsSqlMessageStoreSettings settings)
        {
            _settings = settings;
        }

        public async Task<RecordedStream> ReadStreamForward(string name, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(_settings.ConnectionString))
            using (var cmd = connection.CreateCommandForReadStreamForward(name, fromVersion))
            {
                await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken).NotOnCapturedContext())
                {
                    if (!reader.HasRows) return RecordedStream.Empty();

                    var messages = new List<RecordedMessage>(20);
                    while (await reader.ReadAsync(cancellationToken).NotOnCapturedContext()) // read events
                    {
                        messages.Add(await reader.FillRecordedMessage(cancellationToken).NotOnCapturedContext());
                    }
                    return new RecordedStream(messages.ToArray());
                }
            }
        }

        public async Task AppendToStream(string name, int expectedVersion, UncommittedMessages data, CancellationToken cancellationToken = default(CancellationToken))
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
                    await AppendToStreamWithExpectedVersion(name, ExpectedVersion.NoStream, data, cancellationToken).NotOnCapturedContext();
                    break;
            }
        }

        private async Task AppendToStreamWithExpectedVersion(string name, int expectedVersion, UncommittedMessages data, CancellationToken cancellationToken)
        {
            if (_settings.Batching && data.Messages.Length > 1)
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                using (var batch = new SqlClientSqlCommandSet(connection))
                {
                    var messageVersion = expectedVersion;
                    foreach (var msg in data.Messages)
                    {
                        messageVersion += 1;

                        var cmd = expectedVersion == 0 && messageVersion == 1
                            ? connection.CreateCommandToAppendingWithNoStream(name, data, msg)
                            : connection.CreateCommandToAppendingWithExpectedVersion(name, data, msg, messageVersion);

                        batch.Append(cmd);
                    }

                    await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                    using (var tran = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            batch.Transaction = tran;
                            await batch.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                            tran.Commit();
                        }
                        catch (SqlException e)
                        {
                            tran.Rollback();

                            if (e.IsUniqueConstraintViolation() || e.IsWrongExpectedVersionRised())
                            {
                                throw new WrongExpectedVersionException($"Appending new or existing stream '{name}' error.", e);
                            }
                            throw;
                        }
                    }
                }
            }
            else
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                    using (var tran = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            var messageVersion = expectedVersion;
                            foreach (var msg in data.Messages)
                            {
                                messageVersion += 1;
                                using (var cmd = expectedVersion == 0 && messageVersion == 1
                                    ? connection.CreateCommandToAppendingWithNoStream(name, data, msg)
                                    : connection.CreateCommandToAppendingWithExpectedVersion(name, data, msg, messageVersion))
                                {
                                    cmd.Transaction = tran;
                                    await cmd.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                                }
                            }
                            tran.Commit();
                        }
                        catch (SqlException e)
                        {
                            tran.Rollback();

                            if (e.IsUniqueConstraintViolation() || e.IsWrongExpectedVersionRised())
                            {
                                throw new WrongExpectedVersionException($"Appending new or existing stream '{name}' error.", e);
                            }
                            throw;
                        }
                    }
                }
            }
        }

        private async Task AppendToStreamWithAnyVersion(string name, UncommittedMessages data, CancellationToken cancellationToken)
        {
            if (_settings.Batching && data.Messages.Length > 1)
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                using (var batch = new SqlClientSqlCommandSet(connection))
                {
                    foreach (var msg in data.Messages)
                    {
                        batch.Append(connection.CreateCommandToAppendingWithAnyVersion(name, data, msg));
                    }

                    await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                    using (var tran = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            batch.Transaction = tran;
                            await batch.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                            tran.Commit();
                        }
                        catch (SqlException e)
                        {
                            tran.Rollback();

                            if (e.IsUniqueConstraintViolation() || e.IsWrongExpectedVersionRised())
                            {
                                throw new WrongExpectedVersionException($"Appending new or existing stream '{name}' error.", e);
                            }
                            throw;
                        }
                    }
                }
            }
            else
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                    using (var tran = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            foreach (var msg in data.Messages)
                            {
                                using (var cmd = connection.CreateCommandToAppendingWithAnyVersion(name, data, msg))
                                {
                                    cmd.Transaction = tran;
                                    await cmd.ExecuteNonQueryAsync(cancellationToken).NotOnCapturedContext();
                                }
                            }
                            tran.Commit();
                        }
                        catch (SqlException e)
                        {
                            tran.Rollback();

                            if (e.IsUniqueConstraintViolation() || e.IsWrongExpectedVersionRised())
                            {
                                throw new WrongExpectedVersionException($"Appending new or existing stream '{name}' error.", e);
                            }
                            throw;
                        }
                    }
                }
            }
        }

        public IMessageStoreAdvanced Advanced { get; }
    }
}
