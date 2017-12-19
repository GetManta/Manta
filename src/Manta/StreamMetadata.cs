using System;
using System.Runtime.InteropServices;

namespace Manta
{
    /// <summary>
    /// A struct representing uncommited stream metadata
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StreamMetadata
    {
        public StreamMetadata(int? maxCount, TimeSpan? maxAge, byte[] customPayload)
        {
            MaxCount = maxCount;
            MaxAge = maxAge;
            CustomPayload = customPayload;
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
        public byte[] CustomPayload { get; }
    }
}