using System.Threading;
using System.Threading.Tasks;

namespace Manta.Installer
{
    /// <summary>
    /// Specifies common usage methods for message store installer.
    /// </summary>
    public interface IMessageStoreInstaller
    {
        Task Setup(CancellationToken token = default(CancellationToken));
    }
}