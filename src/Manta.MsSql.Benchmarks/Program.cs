using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manta.Sceleton;

namespace Manta.MsSql.Benchmarks
{
    class Program
    {
        private const string connectionString = "data source=(local); initial catalog = mantabench; Integrated Security = True;";
        private static readonly Random rnd = new Random();
        private static IMessageStore store;

        static void Main(string[] args)
        {
            Console.WriteLine("Manta Benchmarks - {0} batching...", SqlClientSqlCommandSet.IsSqlCommandSetAvailable ? "With" : "Without");
            store = new MsSqlMessageStore(new MsSqlMessageStoreSettings(connectionString));
            var streams = GenerateStreams(250000, 10, out var messagesCount);
            TestMultithreaded(streams, messagesCount).Wait();
            Console.ReadKey();
        }

        public static List<UncommittedMessages> GenerateStreams(int streamsCounter, int maxEventsCounter, out int messagesCount)
        {
            Console.WriteLine("Preparing {0} stream(s) in memory...", streamsCounter);
            var streams = new List<UncommittedMessages>(streamsCounter);
            messagesCount = 0;
            for (var i = 0; i < streamsCounter; i++)
            {
                var messages = GenerateMessages(maxEventsCounter);
                messagesCount += messages.Length;
                streams.Add(new UncommittedMessages(SequentialGuid.NewGuid(), messages));
            }
            Console.WriteLine("Streams prepared. Generated ");
            return streams;
        }

        private static MessageRecord[] GenerateMessages(int maxEventsCounter)
        {
            var msgs = new MessageRecord[rnd.Next(1, maxEventsCounter)];
            for (var i = 1; i <= msgs.Length; i++)
            {
                msgs[i - 1] = new MessageRecord(SequentialGuid.NewGuid(), 1, Encoding.UTF8.GetBytes(string.Join(string.Empty, Enumerable.Range(0, rnd.Next(100, 400)).ToArray())));
            }
            return msgs;
        }

        public static async Task TestMultithreaded(List<UncommittedMessages> streams, int messagesCount)
        {
            var sw = new Stopwatch();
            sw.Start();
            var index = 0;
            const int batch = 5000;
            while (true)
            {
                var tasks = streams.Skip(index).Take(batch).Select(Execute).ToArray();
                Console.WriteLine($"\t{index}...");
                if (tasks.Length == 0) break;
                await Task.WhenAll(tasks);
                index += batch;
            }
            sw.Stop();
            Console.WriteLine($@"Test multithreaded | Overall time {sw.ElapsedMilliseconds}ms - {Math.Round(messagesCount / sw.Elapsed.TotalSeconds, 2, MidpointRounding.AwayFromZero)} event/sec | streams {streams.Count} | avg events per stream {messagesCount / streams.Count} | events {messagesCount}");
        }

        private static async Task Execute(UncommittedMessages data)
        {
            await store.AppendToStream(data.CorrelationId.ToString(), ExpectedVersion.Any, data).NotOnCapturedContext();
        }
    }
}
