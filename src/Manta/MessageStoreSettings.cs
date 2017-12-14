using Manta.Sceleton;
using Manta.Sceleton.Logging;

namespace Manta
{
    public abstract class MessageStoreSettings
    {
        protected MessageStoreSettings(ILogger logger)
        {
            Logger = logger ?? new NullLogger();
        }

        public ILogger Logger { get; protected set; }
        public ILinearizer Linearizer { get; protected set; }
    }
}
