using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manta.Projections
{
    public interface IProjectionCheckpointRepository
    {
        Task<IEnumerable<IProjectionCheckpoint>> Fetch(CancellationToken cancellationToken = default(CancellationToken));
        Task Update(IEnumerable<IProjectionCheckpoint> checkpoints, CancellationToken cancellationToken = default(CancellationToken));
        Task Delete(IEnumerable<IProjectionCheckpoint> checkpoints, CancellationToken cancellationToken = default(CancellationToken));
        Task<IProjectionCheckpoint> AddCheckpoint(string projectorName, string projectionName, CancellationToken cancellationToken = default(CancellationToken));
    }
}