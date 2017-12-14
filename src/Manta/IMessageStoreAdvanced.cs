using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manta
{
    public interface IMessageStoreAdvanced
    {
        Task TruncateStream(string name, int toVersion, CancellationToken cancellationToken = default(CancellationToken));
        Task TruncateStream(string name, DateTime toCreationDate, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteStream(string name, int expectedVersion, bool hardDelete, CancellationToken cancellationToken = default(CancellationToken));

        Task<MessageRecord> ReadSnapshot(string name, CancellationToken cancellationToken = default(CancellationToken));
        Task SaveSnapshot(string name, MessageRecord snapshot, CancellationToken cancellationToken = default(CancellationToken));

        Task<StreamMetadataResult> ReadStreamMetadata(string name, CancellationToken cancellationToken = default(CancellationToken));
        Task SaveStreamMetadata(string name, int expectedVersion, StreamMetadata metadata, CancellationToken cancellationToken = default(CancellationToken));

        Task<long> ReadHeadMessagePosition(CancellationToken cancellationToken = default(CancellationToken));
    }
}