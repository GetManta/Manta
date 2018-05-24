using System;
using System.Collections.Generic;

namespace Manta.Projections
{
    public interface IMetadata
    {
        string StreamId { get; }
        Guid CorrelationId { get; }
        int MessageVersion { get; }
        Guid MessageId { get; }
        long MessagePosition { get; }
        string MessageContractName { get; }
        DateTime Timestamp { get; }
        Dictionary<string, object> CustomMetadata { get; }
    }
}