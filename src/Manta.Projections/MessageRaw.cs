using System;

namespace Manta.Projections
{
    public struct MessageRaw
    {
        public string StreamId;
        public Guid CorrelationId;
        public int MessageVersion;
        public Guid MessageId;
        public long MessagePosition;
        public string MessageContractName;
        public byte[] MessagePayload;
        public byte[] MessageMetadataPayload;
        public DateTime Timestamp;
    }
}
