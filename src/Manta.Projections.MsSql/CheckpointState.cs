using System;

namespace Manta.Projections.MsSql
{
    internal class CheckpointState : IProjectionCheckpoint
    {
        public string ProjectorName { get; set; }
        public string ProjectionName { get; set; }
        public long Position { get; set; }
        public DateTime? DroppedAtUtc { get; set; }
    }
}