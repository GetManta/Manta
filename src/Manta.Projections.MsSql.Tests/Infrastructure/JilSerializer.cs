using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jil;
using Manta.Projections.MsSql.Tests.TestProjections;
using Manta.Sceleton;

namespace Manta.Projections.MsSql.Tests.Infrastructure
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

            using (var ms = new MemoryStream(256))
            using (var writer = new StreamWriter(ms))
            {
                Serialize(message, writer);
                writer.Flush();
                return !ms.TryGetBuffer(out var buffer) ? new ArraySegment<byte>() : buffer;
            }
        }

        public void Serialize(object message, TextWriter writer)
        {
            JSON.Serialize(message, writer, _options);
        }
    }
}