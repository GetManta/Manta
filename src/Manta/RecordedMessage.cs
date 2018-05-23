using System;
using System.IO;

namespace Manta
{
    public class RecordedMessage
    {
        public RecordedMessage(Guid messageId, int version, string contractName, Stream payload)
        {
            MessageId = messageId;
            Version = version;
            ContractName = contractName;
            Payload = payload;
        }

        public Guid MessageId { get; }
        public string ContractName { get; }
        public int Version { get; }
        public Stream Payload { get; }
    }
}