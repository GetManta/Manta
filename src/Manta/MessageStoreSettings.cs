using Manta.Sceleton;
using Manta.Sceleton.Logging;

namespace Manta
{
    public abstract class MessageStoreSettings
    {
        protected MessageStoreSettings()
        {
            Logger = new NullLogger();
        }

        public ILogger Logger { get; protected set; }
    }
}
