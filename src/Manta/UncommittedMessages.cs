using System;

namespace Manta
{
    public class UncommittedMessages
    {
        public UncommittedMessages(Guid correlationId, MessageRecord[] messages, ArraySegment<byte>? commitMetadata = null)
        {
            CorrelationId = correlationId;
            Messages = messages;
            CommitMetadata = commitMetadata;
        }

        public Guid CorrelationId { get; }
        public ArraySegment<byte>? CommitMetadata { get; }
        public MessageRecord[] Messages { get; }
    }
}