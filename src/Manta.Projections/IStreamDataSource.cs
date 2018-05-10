using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Manta.Projections
{
    public interface IStreamDataSource
    {
        Task Fetch(ITargetBlock<MessageRaw> buffer, long fromPosition, int limit, CancellationToken cancellationToken = default(CancellationToken));
    }
}