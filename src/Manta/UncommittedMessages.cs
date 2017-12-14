using System;

namespace Manta
{
    public class UncommittedMessages
    {
        public UncommittedMessages(Guid correlationId, MessageRecord[] messages, byte[] commitMetadata = null)
        {
            CorrelationId = correlationId;
            Messages = messages;
            CommitMetadata = commitMetadata;
        }

        public Guid CorrelationId { get; }
        public byte[] CommitMetadata { get; }
        public MessageRecord[] Messages { get; }
    }
}