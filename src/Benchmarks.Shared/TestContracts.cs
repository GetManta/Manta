using System;
using System.Runtime.Serialization;

namespace Benchmarks.Shared
{
    public class TestContracts
    {
        private static readonly Random rnd = new Random();

        [DataContract(Name = "MessageOne")]
        public class MessageOne
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        [DataContract(Name = "MessageTwo")]
        public class MessageTwo
        {
            public int ID { get; set; }
            public DateTime Date { get; set; }
        }

        public static object RandomContract()
        {
            if (rnd.Next(1, 1000) > 500)
            {
                return new MessageOne
                {
                    Name = Guid.NewGuid().ToString(),
                    ID = rnd.Next(1, int.MaxValue)
                };
            }
            return new MessageTwo
            {
                Date = DateTime.Now,
                ID = rnd.Next(1, int.MaxValue)
            };
        }

        public static Type GetTypeByContractName(string contractName)
        {
            switch (contractName)
            {
                default:
                case "MessageOne":
                    return typeof(MessageOne);
                case "MessageTwo":
                    return typeof(MessageTwo);
            }
        }

        public static string GetContractNameByType(Type type)
        {
            return type.Name;
        }
    }
}
