namespace Manta.Projections.MsSql
{
    public class MsSqlProjector : Projector
    {
        public MsSqlProjector(string name, string connectionString, int batchSize = 1000)
            : base(name, new MsSqlDataSource(connectionString), new MsSqlCheckpointRepository(connectionString), batchSize)
        {
        }
    }
}
