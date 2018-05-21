using System;
using System.Runtime.Serialization;

namespace Benchmarks.Shared
{
    public class TestContracts
    {
        private static readonly Random rnd = new Random();
        private const string pl = " ąśćźżółęńĄŚĆŹŻÓŁĘŃ";

        [DataContract(Name = "MessageOne")]
        public class MessageOne
        {
            public long ID { get; set; }
            public string Name { get; set; }
        }

        [DataContract(Name = "MessageTwo")]
        public class MessageTwo
        {
            public long ID { get; set; }
            public DateTime Date { get; set; }
        }

        public static object RandomContract()
        {
            if (rnd.Next(1, 1000) > 500)
            {
                return new MessageOne
                {
                    Name = Guid.NewGuid() + pl,
                    ID = DateTime.UtcNow.Ticks
                };
            }
            return new MessageTwo
            {
                Date = DateTime.Now,
                ID = DateTime.UtcNow.Ticks
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
