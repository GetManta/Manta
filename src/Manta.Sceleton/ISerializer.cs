using System;
using System.IO;

namespace Manta.Sceleton
{
    public interface ISerializer
    {
        object Deserialize(string contractName, TextReader reader);
        T Deserialize<T>(TextReader reader);

        ArraySegment<byte> Serialize(object message);
        void Serialize(object message, TextWriter writer);
    }
}