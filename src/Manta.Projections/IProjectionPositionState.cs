namespace Manta.Projections
{
    public interface IProjectionCheckpoint
    {
        string ProjectorName { get; }
        string ProjectionName { get; }
        long Position { get; }
    }
}