﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manta.Projections
{
    public interface IProjectionCheckpointRepository
    {
        Task<IEnumerable<IProjectionCheckpoint>> Fetch(CancellationToken token = default(CancellationToken));
        Task Update(IProjectionCheckpoint checkpoint, bool undropRequested, CancellationToken token = default(CancellationToken));
        Task Delete(IProjectionCheckpoint[] checkpoints, CancellationToken token = default(CancellationToken));
        Task<IProjectionCheckpoint> AddCheckpoint(string projectorName, string projectionName, CancellationToken token = default(CancellationToken));
    }
}