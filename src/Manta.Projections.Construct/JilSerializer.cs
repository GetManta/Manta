using System.Collections.Generic;
using System.IO;
using System.Text;
using Jil;
using Manta.Projections.Construct.TestProjections;
using Manta.Sceleton;

namespace Manta.Projections.Construct
{
    public class JilSerializer : ISerializer
    {
        private readonly Options _options;

        public JilSerializer()
        {
            _options = new Options(dateFormat: DateTimeFormat.ISO8601);
        }

        public object DeserializeMessage(string messageContractName, byte[] payload)
        {
            if (payload == null || payload.Length == 0) return null;

            var type = TestContracts.GetTypeByContractName(messageContractName);
            using (var output = new StreamReader(new MemoryStream(payload), Encoding.UTF8))
            {
                return JSON.Deserialize(output, type);
            }
        }

        public Dictionary<string, object> DeserializeMetadata(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return null;

            using (var output = new StreamReader(new MemoryStream(payload), Encoding.UTF8))
            {
                return JSON.Deserialize<Dictionary<string, object>>(output);
            }
        }

        public byte[] SerializeMessage(object message)
        {
            using (var mem = new MemoryStream(30))
            {
                using (var input = new StreamWriter(mem))
                {
                    JSON.Serialize(message, input, _options);
                }
                return mem.GetBuffer();
            }
        }

        public byte[] SerializeMetadata(Dictionary<string, object> metadata)
        {
            using (var mem = new MemoryStream(30))
            {
                using (var input = new StreamWriter(mem))
                {
                    JSON.Serialize(metadata, input, _options);
                }
                return mem.GetBuffer();
            }
        }
    }
}