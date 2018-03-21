using System;

namespace Manta
{
    public class MessageRecord
    {
        public const byte DefaultContractNameLength = 128;

        public MessageRecord(Guid messageId, string contractName, byte[] payload)
        {
            MessageId = messageId;
            ContractName = contractName ?? throw new ArgumentNullException(nameof(contractName));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        public Guid MessageId { get; }
        public string ContractName { get; }
        public byte[] Payload { get; }
    }
}