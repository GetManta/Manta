namespace Manta
{
    public struct RecordedStream
    {
        public RecordedStream(MessageRecord[] messages)
        {
            CommittedVersion = CalculateCurrentVersion(messages);
            Messages = messages;
        }

        private static int CalculateCurrentVersion(MessageRecord[] messages)
        {
            return messages[messages.Length - 1].Version;
        }

        public MessageRecord[] Messages { get; }
        public int CommittedVersion { get; }
    }
}