using System;
using System.Collections.Generic;

namespace Manta.Projections
{
    public class Metadata
    {
        public string StreamId;
        public Guid CorrelationId;
        public int MessageVersion;
        public Guid MessageId;
        public long MessagePosition;
        public string MessageContractName;
        public DateTime Timestamp;
        public Dictionary<string, object> CustomMetadata;
    }
}