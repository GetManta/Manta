using System;
using System.Collections.Generic;

namespace Manta.Projections
{
    public class MessageEnvelope : IMetadata
    {
        private readonly Dictionary<string, object> _customMetadata;
        private readonly MessageRaw _raw;

        public MessageEnvelope(object message, Dictionary<string, object> customMetadata, MessageRaw raw)
        {
            Message = message;
            _customMetadata = customMetadata;
            _raw = raw;

            _raw.MessagePayload?.Dispose();
            _raw.MessagePayload = null;
            _raw.MessageMetadataPayload?.Dispose();
            _raw.MessageMetadataPayload = null;
        }

        public IMetadata Meta => this;
        public object Message { get; }

        string IMetadata.StreamId => _raw.StreamId;
        Guid IMetadata.CorrelationId => _raw.CorrelationId;
        int IMetadata.MessageVersion => _raw.MessageVersion;
        Guid IMetadata.MessageId => _raw.MessageId;
        long IMetadata.MessagePosition => _raw.MessagePosition;
        string IMetadata.MessageContractName => _raw.MessageContractName;
        DateTime IMetadata.Timestamp => _raw.Timestamp;
        Dictionary<string, object> IMetadata.CustomMetadata => _customMetadata;
    }
}