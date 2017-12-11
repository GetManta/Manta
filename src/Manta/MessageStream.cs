namespace Manta
{
    public struct RecordedStream
    {
        public const sbyte StreamNotFoundVersion = -1;

        public RecordedStream(RecordedMessage[] messages)
        {
            CommittedVersion = CalculateCurrentVersion(messages);
            Messages = messages;
        }

        private static int CalculateCurrentVersion(RecordedMessage[] messages)
        {
            return messages.Length > 0 ? messages[messages.Length - 1].Version : StreamNotFoundVersion;
        }

        public RecordedMessage[] Messages { get; }
        public int CommittedVersion { get; }

        public bool IsStreamNotFound()
        {
            return CommittedVersion == StreamNotFoundVersion;
        }

        public static RecordedStream Empty()
        {
            return new RecordedStream(new RecordedMessage[0]);
        }
    }
}