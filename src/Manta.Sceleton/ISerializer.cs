using System.Collections.Generic;

namespace Manta.Sceleton
{
    public interface ISerializer
    {
        object DeserializeMessage(string messageContractName, byte[] messagePayload);
        Dictionary<string, object> DeserializeMetadata(byte[] messageMetadataPayload);
        byte[] SerializeMessage(object message);
        byte[] SerializeMetadata(Dictionary<string, object> metadata);
    }
}