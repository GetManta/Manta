using Manta.Sceleton.Installer;

namespace Manta.Projections.Installer
{
    /// <inheritdoc />
    public abstract class ProjectorsInstaller : BaseInstaller
    {
        protected ProjectorsInstaller() : base(typeof(Projector).Assembly) { }
    }
}