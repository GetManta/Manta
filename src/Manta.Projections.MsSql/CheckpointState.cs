namespace Manta.Projections.MsSql
{
    internal class CheckpointState : IProjectionCheckpoint
    {
        public string ProjectorName { get; private set; }
        public string ProjectionName { get; private set; }
        public long Position { get; private set; }
    }
}