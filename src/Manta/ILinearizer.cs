using System;
using System.Threading.Tasks;

namespace Manta
{
    public interface ILinearizer
    {
        void Start();
        Task RunNow();
        void Stop();
        bool IsRunning { get; }
        TimeSpan WorkDuration { get; }
        TimeSpan Timeout { get; }
    }
}