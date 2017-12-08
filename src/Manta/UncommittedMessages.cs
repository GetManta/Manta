using System;

namespace Manta
{
    public struct UncommittedMessages
    {
        public UncommittedMessages(Guid correlationId, MessageRecord[] messages, byte[] metadata = null)
        {
            CorrelationId = correlationId;
            Messages = messages;
            CommitMetadata = metadata;
        }

        public Guid CorrelationId { get; }
        public byte[] CommitMetadata { get; }
        public MessageRecord[] Messages { get; }
    }
}