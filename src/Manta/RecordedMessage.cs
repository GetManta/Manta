using System;

namespace Manta
{
    public struct RecordedMessage
    {
        public RecordedMessage(Guid messageId, int contractId, int version, byte[] payload)
        {
            MessageId = messageId;
            ContractId = contractId;
            Version = version;
            Payload = payload;
        }

        public Guid MessageId { get; }
        public int ContractId { get; }
        public int Version { get; }
        public byte[] Payload { get; }
    }
}