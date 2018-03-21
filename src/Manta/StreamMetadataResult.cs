namespace Manta
{
    public class StreamMetadataResult
    {
        /// <summary>
        /// The identifier of the stream.
        /// </summary>
        public string StreamName { get; }

        /// <summary>
        /// True if the stream is soft-deleted.
        /// </summary>
        public bool IsStreamDeleted { get; }

        /// <summary>
        /// The version of the metadata format.
        /// </summary>
        public long Version { get; }

        /// <summary>
        /// A <see cref="StreamMetadata"/> containing user-specified metadata.
        /// </summary>
        public StreamMetadata Metadata { get; }

        public StreamMetadataResult(string streamName, int version, bool isStreamDeleted, StreamMetadata metadata)
        {
            StreamName = streamName;
            Version = version;
            IsStreamDeleted = isStreamDeleted;
            Metadata = metadata;
        }
    }
}