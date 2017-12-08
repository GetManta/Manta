namespace Manta.MsSql
{
    public class MsSqlMessageStoreSettings : MessageStoreSettings
    {
        public MsSqlMessageStoreSettings(string connectionString, int batchSize = 0)
        {
            ConnectionString = connectionString;
            BatchSize = batchSize;
        }

        public int BatchSize { get; }
        public string ConnectionString { get; }
    }
}