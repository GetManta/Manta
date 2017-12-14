using System;
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
            CheckForBatchingAvailability();
            Advanced = new MsSqlMessageStoreAdvanced(_settings);
        }

        private void CheckForBatchingAvailability()
        {
            if (SqlClientSqlCommandSet.IsSqlCommandSetAvailable)
            {
                _settings.Logger.Info("Batching is not available.");
            }
        }

        public async Task<RecordedStream> ReadStreamForward(string stream, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            _settings.Logger.Trace("Reading stream '{0}' forward from version {1}...", stream, fromVersion);

            using (var connection = new SqlConnection(_settings.ConnectionString))
            using (var cmd = connection.CreateCommandForReadStreamForward(stream, fromVersion))
            {
                await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult, cancellationToken).NotOnCapturedContext())
                {
                    if (!reader.HasRows)
                    {
                        _settings.Logger.Trace("Read 0 messages for '{0}' stream from version {1}.", stream, fromVersion);
                        return RecordedStream.Empty();
                    }

                    var messages = new List<RecordedMessage>(20); // 20? How many will be enough?
                    while (await reader.ReadAsync(cancellationToken).NotOnCapturedContext())
                    {
                        messages.Add(await reader.GetRecordedMessage(cancellationToken).NotOnCapturedContext());
                    }
                    _settings.Logger.Trace("Read {0} messages for '{1}' stream from version {2}.", messages.Count, stream, fromVersion);
                    return new RecordedStream(messages.ToArray());
                }
            }
        }

        public async Task AppendToStream(string stream, int expectedVersion, UncommittedMessages data, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream.IsNullOrEmpty()) throw new ArgumentNullException(nameof(stream));
            if (expectedVersion < ExpectedVersion.Any) throw new ArgumentException("Expected version must be greater or equal 1, or 'Any', or 'NoStream'.", nameof(expectedVersion));
            if (data == null) throw new ArgumentNullException(nameof(data));

            try
            {
                switch (expectedVersion)
                {
                    default:
                        _settings.Logger.Trace("Appending {0} messages to stream '{1}' with expected version {2}...", data.Messages.Length, stream, expectedVersion);
                        await AppendToStreamWithExpectedVersion(stream, expectedVersion, data, cancellationToken).NotOnCapturedContext();
                        break;
                    case ExpectedVersion.Any:
                        _settings.Logger.Trace("Appending {0} messages to stream '{1}' with any version...", data.Messages.Length, stream);
                        await AppendToStreamWithAnyVersion(stream, data, cancellationToken).NotOnCapturedContext();
                        break;
                    case ExpectedVersion.NoStream:
                        _settings.Logger.Trace("Appending {0} messages to stream '{1}' where stream not existed yet...", data.Messages.Length, stream);
                        await AppendToStreamWithExpectedVersion(stream, ExpectedVersion.NoStream, data, cancellationToken).NotOnCapturedContext();
                        break;
                }

                _settings.Linearizer?.Start();
            }
            catch (WrongExpectedVersionException)
            {
                throw;
            }
            catch (Exception e)
            {
                _settings.Logger.Error(e.ToString());
            }
        }

        private async Task AppendToStreamWithExpectedVersion(string stream, int expectedVersion, UncommittedMessages data, CancellationToken cancellationToken)
        {
            if (SqlClientSqlCommandSet.IsSqlCommandSetAvailable && _settings.Batching && data.Messages.Length > 1)
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                using (var batch = new SqlClientSqlCommandSet(connection))
                {
                    var messageVersion = expectedVersion;
                    foreach (var msg in data.Messages)
                    {
                        messageVersion += 1;

                        var cmd = expectedVersion == 0 && messageVersion == 1
                            ? connection.CreateCommandToAppendingWithNoStream(stream, data, msg)
                            : connection.CreateCommandToAppendingWithExpectedVersion(stream, data, msg, messageVersion);

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
                                throw new WrongExpectedVersionException($"Appending {data.Messages.Length} messages to stream '{stream}' with expected version {ExpectedVersion.Parse(expectedVersion)} failed.", e);
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
                                    ? connection.CreateCommandToAppendingWithNoStream(stream, data, msg)
                                    : connection.CreateCommandToAppendingWithExpectedVersion(stream, data, msg, messageVersion))
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
                                throw new WrongExpectedVersionException($"Appending {data.Messages.Length} messages to stream '{stream}' with expected version {ExpectedVersion.Parse(expectedVersion)} failed.", e);
                            }
                            throw;
                        }
                    }
                }
            }
        }

        private async Task AppendToStreamWithAnyVersion(string stream, UncommittedMessages data, CancellationToken cancellationToken)
        {
            _settings.Logger.Trace("Appending {0} messages to stream '{1}' with any version...", data.Messages.Length, stream);

            if (SqlClientSqlCommandSet.IsSqlCommandSetAvailable && _settings.Batching && data.Messages.Length > 1)
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                using (var batch = new SqlClientSqlCommandSet(connection))
                {
                    foreach (var msg in data.Messages)
                    {
                        batch.Append(connection.CreateCommandToAppendingWithAnyVersion(stream, data, msg));
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
                                throw new WrongExpectedVersionException($"Appending {data.Messages.Length} messages to stream '{stream}' with any version failed.", e);
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
                                using (var cmd = connection.CreateCommandToAppendingWithAnyVersion(stream, data, msg))
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
                                throw new WrongExpectedVersionException($"Appending {data.Messages.Length} messages to stream '{stream}' with any version failed.", e);
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
