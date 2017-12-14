using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manta
{
    public interface IMessageStoreAdvanced
    {
        Task TruncateStream(string stream, int toVersion, CancellationToken cancellationToken = default(CancellationToken));
        Task TruncateStream(string stream, DateTime toCreationDate, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteStream(string stream, int expectedVersion, bool hardDelete, CancellationToken cancellationToken = default(CancellationToken));

        Task<RecordedMessage?> ReadMessage(string stream, int messageVersion, CancellationToken cancellationToken = default(CancellationToken));

        Task<MessageRecord> ReadSnapshot(string stream, CancellationToken cancellationToken = default(CancellationToken));
        Task SaveSnapshot(string stream, MessageRecord snapshot, CancellationToken cancellationToken = default(CancellationToken));

        Task<StreamMetadataResult> ReadStreamMetadata(string stream, CancellationToken cancellationToken = default(CancellationToken));
        Task SaveStreamMetadata(string stream, int expectedVersion, StreamMetadata metadata, CancellationToken cancellationToken = default(CancellationToken));

        Task<long> ReadHeadMessagePosition(CancellationToken cancellationToken = default(CancellationToken));
    }
}