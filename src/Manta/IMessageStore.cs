using System.Threading;
using System.Threading.Tasks;

namespace Manta
{
    public interface IMessageStore
    {
        Task<RecordedStream> ReadStreamForward(string name, int fromVersion, CancellationToken cancellationToken = default(CancellationToken));
        Task AppendToStream(string name, int expectedVersion, UncommittedMessages data, CancellationToken cancellationToken = default(CancellationToken));

        IMessageStoreAdvanced Advanced { get; }
    }
}