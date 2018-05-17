using Manta.Sceleton;

namespace Manta.Projections.MsSql
{
    public class MsSqlProjector : Projector
    {
        public MsSqlProjector(string name, string connectionString, ISerializer serializer, int batchSize = 1000)
            : base(name, new MsSqlDataSource(connectionString), new MsSqlCheckpointRepository(connectionString), serializer, batchSize)
        {
        }
    }
}
