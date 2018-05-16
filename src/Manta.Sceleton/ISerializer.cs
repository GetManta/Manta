using System;
using System.Collections.Generic;

namespace Manta.Sceleton
{
    public interface ISerializer
    {
        object DeserializeMessage(string messageContractName, byte[] messagePayload);
        Dictionary<string, object> DeserializeMetadata(byte[] messageMetadataPayload);
        ArraySegment<byte> SerializeMessage(object message);
        ArraySegment<byte> SerializeMetadata(Dictionary<string, object> metadata);
    }
}