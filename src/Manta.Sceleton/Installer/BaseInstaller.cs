using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Manta.Sceleton.Installer
{
    /// <summary>
    /// Specifies common usage methods for message store installer.
    /// </summary>
    public abstract class BaseInstaller
    {
        private readonly Assembly _assembly;

        protected BaseInstaller(Assembly moduleAssembly) => _assembly = moduleAssembly ?? throw new ArgumentNullException(nameof(moduleAssembly));

        public async Task Execute(CancellationToken token = default(CancellationToken))
        {
            var installedVersion = await GetInstalledVersion(token).NotOnCapturedContext();
            if (ShouldInstall(installedVersion))
            {
                await Install(installedVersion, token).NotOnCapturedContext();
            }
        }

        protected abstract Task Install(Version installedVersion, CancellationToken token = default(CancellationToken));
        protected abstract Task<Version> GetInstalledVersion(CancellationToken token = default(CancellationToken));

        protected Version GetModuleVersion() => _assembly.GetName().Version;
        protected bool ShouldInstall(Version currentVersion) => currentVersion == null || GetModuleVersion() > currentVersion;
    }
}