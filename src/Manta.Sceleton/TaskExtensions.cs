using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Manta.Sceleton
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable<T> NotOnCapturedContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable NotOnCapturedContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static void SwallowException(this Task task, ILogger logger = null)
        {
            task.ContinueWith(
                x =>
                {
                    if (x.Exception?.InnerException != null)
                    {
                        logger?.Error(x.Exception.InnerException.Message);
                    }
                });
        }
    }
}