using System.Threading;
using System.Threading.Tasks;
using Manta.Installer;
using Manta.Sceleton;

namespace Manta.MsSql.Installer
{
    public class MsSqlMessageStoreInstaller : IMessageStoreInstaller
    {
        private readonly string _connectionString;

        public MsSqlMessageStoreInstaller(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Setup(CancellationToken token = default(CancellationToken))
        {
            // Check for version
            // if no any install fresh
            // if exists and need update then execute update scripts and change version

            var currentVersion = await GetVersion(token).NotOnCapturedContext();
            if (currentVersion == null)
            {
                await Install(token).NotOnCapturedContext();
            }
            else
            {
                await Upgrade(currentVersion, token).NotOnCapturedContext();
            }
        }

        private Task Install(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private Task Upgrade(string currentVersion, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task<string> GetVersion(CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult("1.0");
        }

        public Task SetVersion(string version, CancellationToken token = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }

    // http://blogs.lessthandot.com/index.php/datamgmt/datadesign/document-your-sql-server-databases/
}