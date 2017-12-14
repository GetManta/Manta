using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Manta.Sceleton;

namespace Manta.MsSql
{
    public class MsSqlMessageStoreAdvanced : IMessageStoreAdvanced
    {
        private readonly MsSqlMessageStoreSettings _settings;

        public MsSqlMessageStoreAdvanced(MsSqlMessageStoreSettings settings)
        {
            _settings = settings;
        }

        public Task TruncateStream(string name, int toVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task TruncateStream(string name, DateTime toCreationDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task DeleteStream(string name, int expectedVersion, bool hardDelete, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<MessageRecord> ReadSnapshot(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task SaveSnapshot(string name, MessageRecord snapshot, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<StreamMetadataResult> ReadStreamMetadata(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task SaveStreamMetadata(string name, int expectedVersion, StreamMetadata metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public async Task<long> ReadHeadMessagePosition(CancellationToken cancellationToken = default(CancellationToken))
        {
            _settings.Logger.Trace("Reading head message position...");

            using (var connection = new SqlConnection(_settings.ConnectionString))
            using (var cmd = connection.CreateCommandForReadHeadMessagePosition())
            {
                await connection.OpenAsync(cancellationToken).NotOnCapturedContext();
                var head = await cmd.ExecuteScalarAsync(cancellationToken).NotOnCapturedContext() ?? 0;
                if (head == DBNull.Value) head = 0;
                _settings.Logger.Trace("Read {0} head message position.", head);
                return (long)head;
            }
        }
    }
}