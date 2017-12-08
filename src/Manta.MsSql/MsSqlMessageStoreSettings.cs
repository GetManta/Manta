namespace Manta.MsSql
{
    public class MsSqlMessageStoreSettings : MessageStoreSettings
    {
        public MsSqlMessageStoreSettings(string connectionString, bool batching = true)
        {
            ConnectionString = connectionString;
            Batching = batching;
        }

        public bool Batching { get; }
        public string ConnectionString { get; }
    }
}