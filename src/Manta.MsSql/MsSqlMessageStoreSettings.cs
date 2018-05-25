using System;

namespace Manta.MsSql
{
    public class MsSqlMessageStoreSettings : MessageStoreSettings
    {
        public MsSqlMessageStoreSettings(string connectionString, bool batching = true)
            : base(null)
        {
            ConnectionString = connectionString;
            Batching = batching;
        }

        public bool Batching { get; }
        public string ConnectionString { get; }

        public MsSqlMessageStoreSettings WithLinearizer(TimeSpan timeout, TimeSpan workDuration)
        {
            Linearizer = new MsSqlLinearizer(ConnectionString, Logger, timeout, workDuration);
            return this;
        }

        public MsSqlMessageStoreSettings WithLinearizer()
        {
            Linearizer = new MsSqlLinearizer(ConnectionString, Logger);
            return this;
        }
    }
}