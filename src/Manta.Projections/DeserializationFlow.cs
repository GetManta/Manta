using System;
using System.Collections.Generic;
using System.IO;
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

            return new MessageEnvelope(message, customMetadata, raw);
        }

        private static object DeserializeMessage(ISerializer serializer, MessageRaw raw)
        {
            if (raw.MessagePayload == null) return null;

            using (var reader = new StreamReader(raw.MessagePayload))
            {
                return serializer.Deserialize(raw.MessageContractName, reader);
            }
        }

        private static Dictionary<string, object> DeserializeMetadata(ISerializer serializer, MessageRaw raw)
        {
            if (raw.MessageMetadataPayload == null) return null;

            using (var reader = new StreamReader(raw.MessageMetadataPayload))
            {
                return serializer.Deserialize<Dictionary<string, object>>(reader);
            }
        }

        private static object UpConvert(IUpConverterFactory upConverterFactory, Type messageType, object message)
        {
            var converter = upConverterFactory.CreateInstanceFor(messageType);
            while (converter != null)
            {
                message = upConverterFactory.Execute(converter, messageType, message);
                converter = upConverterFactory.CreateInstanceFor(message.GetType());
            }
            return message;
        }
    }
}