using System;

namespace Manta
{
    /// <summary>
    /// A struct representing stream metadata
    /// </summary>
    public struct StreamMetadata
    {
        public StreamMetadata(int? maxCount, TimeSpan? maxAge, byte[] payload)
        {
            MaxCount = maxCount;
            MaxAge = maxAge;
            Payload = payload;
        }

        /// <summary>
        /// The maximum number of events allowed in the stream.
        /// </summary>
        public int? MaxCount { get; }

        /// <summary>
        /// The maximum age of events allowed in the stream.
        /// </summary>
        public TimeSpan? MaxAge { get; }

        /// <summary>
        /// Custom metadata payload
        /// </summary>
        public byte[] Payload { get; }
    }
}