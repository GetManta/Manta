using System;

namespace Manta
{
    public class UncommittedMessages
    {
        public UncommittedMessages(Guid correlationId, MessageRecord[] messages, ArraySegment<byte>? metadata = null)
        {
            CorrelationId = correlationId;
            Messages = messages;
            Metadata = metadata;
        }

        public Guid CorrelationId { get; }
        public ArraySegment<byte>? Metadata { get; }
        public MessageRecord[] Messages { get; }
    }
}