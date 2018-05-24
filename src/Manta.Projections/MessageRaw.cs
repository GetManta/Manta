using System;
using System.IO;

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
        public Stream MessagePayload;
        public Stream MessageMetadataPayload;
        public DateTime Timestamp;
    }
}
