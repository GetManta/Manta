using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Manta.Sceleton;
using Manta.Sceleton.Converters;

namespace Manta.Projections
{
    internal class DeserializationFlow
    {
        public static MessageEnvelope Transform(ISerializer serializer, IUpConverterFactory upConverterFactory, MessageRaw raw)
        {
            var message = DeserializeMessage(serializer, raw);
            var customMetadata = DeserializeMetadata(serializer, raw);

            if (upConverterFactory != null)
            {
                message = UpConvert(upConverterFactory, message.GetType(), message);
            }

            var metadata = new Metadata
            {
                CustomMetadata = customMetadata,
                CorrelationId = raw.CorrelationId,
                MessageContractName = raw.MessageContractName,
                MessageId = raw.MessageId,
                MessagePosition = raw.MessagePosition,
                MessageVersion = raw.MessageVersion,
                StreamId = raw.StreamId,
                Timestamp = raw.Timestamp
            };

            return new MessageEnvelope(metadata, message);
        }

        private static object DeserializeMessage(ISerializer serializer, MessageRaw raw)
        {
            if (raw.MessagePayload == null) return null;

            using (var reader = new StreamReader(new MemoryStream(raw.MessagePayload), Encoding.UTF8))
            {
                return serializer.Deserialize(raw.MessageContractName, reader);
            }
        }

        private static Dictionary<string, object> DeserializeMetadata(ISerializer serializer, MessageRaw raw)
        {
            if (raw.MessageMetadataPayload == null) return null;

            using (var reader = new StreamReader(new MemoryStream(raw.MessageMetadataPayload), Encoding.UTF8))
            {
                return serializer.Deserialize<Dictionary<string, object>>(reader);
            }
        }

        private static object UpConvert(IUpConverterFactory upConverterFactory, Type messageType, object message)
        {
            var upConverter = upConverterFactory.CreateInstanceFor(messageType);
            while (upConverter != null)
            {
                message = ((dynamic)upConverter).Convert((dynamic)message);
                upConverter = upConverterFactory.CreateInstanceFor(message.GetType());
            }
            return message;
        }
    }
}