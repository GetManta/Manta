namespace Manta
{
    public class StreamMetadataResult
    {
        /// <summary>
        /// The name of the stream.
        /// </summary>
        public string Stream { get; }

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

        public StreamMetadataResult(string stream, int version, bool isStreamDeleted, StreamMetadata metadata)
        {
            Stream = stream;
            Version = version;
            IsStreamDeleted = isStreamDeleted;
            Metadata = metadata;
        }
    }
}