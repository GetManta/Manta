using System.Threading;
using System.Threading.Tasks;

namespace Manta
{
    /// <summary>
    /// Specifies common usage methods for message store.
    /// </summary>
    public interface IMessageStore
    {
        /// <summary>
        /// Reads all of the messages for the named stream
        /// </summary>
        /// <param name="stream">Name of the stream</param>
        /// <param name="fromVersion">Minimum message version to read from</param>
        /// <param name="token">Cancelation token</param>
        /// <returns>Recorded stream with messages</returns>
        Task<RecordedStream> ReadStreamForward(string stream, int fromVersion, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Append one or more messages in order to the named stream.
        /// </summary>
        /// <param name="stream">Name of the stream</param>
        /// <param name="expectedVersion">Expected stream version (maximum message version) before append.</param>
        /// <param name="data">Set of uncommited events with specified CorrelationId and/or commit metadata.</param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task AppendToStream(string stream, int expectedVersion, UncommittedMessages data, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Returns advanced usage methods for message store.
        /// </summary>
        IMessageStoreAdvanced Advanced { get; }
    }
}