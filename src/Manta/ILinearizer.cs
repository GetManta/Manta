using System;
using System.Threading.Tasks;

namespace Manta
{
    /// <summary>
    /// Provides a way to linearize entire log of messages.
    /// </summary>
    public interface ILinearizer
    {
        /// <summary>
        /// Starting Linearizer.
        /// </summary>
        void Start();

        /// <summary>
        /// Run once and exit when done.
        /// </summary>
        /// <returns>Task</returns>
        Task Run();

        /// <summary>
        /// Stopping Linearizer.
        /// </summary>
        void Stop();

        /// <summary>
        /// Returns whether the Linearizer is working.
        /// </summary>
        bool IsWorking { get; }

        /// <summary>
        /// Returns how long Linearizer should work.
        /// </summary>
        TimeSpan WorkDuration { get; }

        /// <summary>
        /// Returns how often Linearizer should poll for new work.
        /// </summary>
        TimeSpan Timeout { get; }
    }
}