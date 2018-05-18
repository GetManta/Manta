using System;

namespace Manta
{
    public struct MessageRecord
    {
        public MessageRecord(Guid messageId, string contractName, ArraySegment<byte> payload)
        {
            MessageId = messageId;
            ContractName = contractName ?? throw new ArgumentNullException(nameof(contractName));
            Payload = payload;
        }

        public Guid MessageId { get; }
        public string ContractName { get; }
        public ArraySegment<byte> Payload { get; }
    }
}