using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manta
{
    /// <summary>
    /// Specifies advanced usage methods for message store.
    /// </summary>
    public interface IMessageStoreAdvanced
    {
        /// <summary>
        /// Truncates the stream up to specified message version.
        /// </summary>
        /// <remarks>
        /// Use it wisely. Truncating can dramatically decrease database performance.
        /// </remarks>
        /// <param name="stream">Name of the stream</param>
        /// <param name="expectedVersion">Expected version of the stream before deleting</param>
        /// <param name="toVersion">Message version</param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task TruncateStream(string stream, int expectedVersion, int toVersion, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Truncates the stream up to specified creation (UTC) date.
        /// </summary>
        /// <remarks>
        /// Use it wisely. Truncating can dramatically decrease database performance.
        /// </remarks>
        /// <param name="stream">Name of the stream</param>
        /// <param name="expectedVersion">Expected version of the stream before deleting</param>
        /// <param name="toCreationDate">Creation date (UTC) of message</param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task TruncateStream(string stream, int expectedVersion, DateTime toCreationDate, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Deletes the stream with specified message version.
        /// </summary>
        /// <remarks>
        /// Use it wisely. Deleting can dramatically decrease database performance.
        /// </remarks>
        /// <param name="stream">Name of the stream</param>
        /// <param name="expectedVersion">Expected version of the stream before deleting</param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task DeleteStream(string stream, int expectedVersion, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Name of the stream</param>
        /// <param name="messageVersion"></param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task<RecordedMessage> ReadMessage(string stream, int messageVersion, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Name of the stream</param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task<MessageRecord> ReadSnapshot(string stream, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Name of the stream</param>
        /// <param name="snapshot"></param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task SaveSnapshot(string stream, MessageRecord snapshot, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Name of the stream</param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task<StreamMetadataResult> ReadStreamMetadata(string stream, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Name of the stream</param>
        /// <param name="expectedVersion"></param>
        /// <param name="metadata"></param>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task SaveStreamMetadata(string stream, int expectedVersion, StreamMetadata metadata, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token">Cancelation token</param>
        /// <returns></returns>
        Task<long> ReadHeadMessagePosition(CancellationToken token = default(CancellationToken));
    }
}