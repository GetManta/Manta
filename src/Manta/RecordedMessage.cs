using System;
using System.Runtime.InteropServices;

namespace Manta
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RecordedMessage
    {
        public RecordedMessage(Guid messageId, int version, int contractId, byte[] payload)
        {
            MessageId = messageId;
            Version = version;
            ContractId = contractId;
            Payload = payload;
        }

        public Guid MessageId { get; }
        public int ContractId { get; }
        public int Version { get; }
        public byte[] Payload { get; }
    }
}