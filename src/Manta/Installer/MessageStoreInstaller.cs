using Manta.Sceleton.Installer;

namespace Manta.Installer
{
    /// <inheritdoc />
    public abstract class MessageStoreInstaller : BaseInstaller
    {
        protected MessageStoreInstaller() : base(typeof(IMessageStore).Assembly) { }
    }
}