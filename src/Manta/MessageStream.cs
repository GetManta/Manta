namespace Manta
{
    public struct RecordedStream
    {
        public RecordedStream(RecordedMessage[] messages)
        {
            CommittedVersion = CalculateCurrentVersion(messages);
            Messages = messages;
        }

        private static int CalculateCurrentVersion(RecordedMessage[] messages)
        {
            return messages[messages.Length - 1].Version;
        }

        public RecordedMessage[] Messages { get; }
        public int CommittedVersion { get; }
    }
}