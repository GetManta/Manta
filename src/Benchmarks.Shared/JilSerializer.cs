using System;
using System.IO;
using System.Text;
using Jil;
using Manta.Sceleton;

namespace Benchmarks.Shared
{
    public class JilSerializer : ISerializer
    {
        private readonly Options _options;

        public JilSerializer()
        {
            _options = new Options(dateFormat: DateTimeFormat.ISO8601);
        }

        public object Deserialize(string contractName, TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var type = TestContracts.GetTypeByContractName(contractName);
            return JSON.Deserialize(reader, type, _options);
        }

        public T Deserialize<T>(TextReader reader)
        {
            return JSON.Deserialize<T>(reader, _options);
        }

        public ArraySegment<byte> Serialize(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, Encoding.UTF8))
            {
                Serialize(message, writer);
                writer.Flush();
                return !ms.TryGetBuffer(out var buffer) ? new ArraySegment<byte>() : buffer;
            }
        }

        public void Serialize(object message, TextWriter writer)
        {
            if (message == null) return;
            JSON.Serialize(message, writer, _options);
        }
    }
}