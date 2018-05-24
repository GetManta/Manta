using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Benchmarks.Shared;
using Manta.MsSql.Installer;
using Manta.Sceleton;

namespace Manta.MsSql.Benchmarks
{
    internal class Program
    {
        private const string connectionString = "data source=(local); initial catalog = mantabench; Integrated Security = True; Enlist = false;";
        private static readonly Random rnd = new Random();
        private static IMessageStore store;
        private static ISerializer serializer;

        private static void Main()
        {
            Console.WriteLine("Manta Benchmarks ({0})");
            Console.WriteLine("{0} ({1})", RuntimeInformation.OSDescription, RuntimeInformation.OSArchitecture);

            Install().Wait();

            serializer = new JilSerializer();
            var streams = GenerateStreams(250000, 10, out var messagesCount);
            store = new MsSqlMessageStore(new MsSqlMessageStoreSettings(connectionString));

            MultithreadedAppendingTest(streams, messagesCount).Wait();
            MultithreadedReadingTest(streams.OrderBy(x => Guid.NewGuid()).ToList(), messagesCount).Wait();
            Console.ReadKey();
        }

        private static async Task Install()
        {
            var installer = new MsSqlMessageStoreInstaller(connectionString);
            await installer.Execute();
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
                var contract = TestContracts.RandomContract();
                var contractName = TestContracts.GetContractNameByType(contract.GetType());

                msgs[i - 1] = new MessageRecord(SequentialGuid.NewGuid(), contractName, serializer.Serialize(contract));
            }

            return msgs;
        }

        public static async Task MultithreadedAppendingTest(List<UncommittedMessages> streams, int messagesCount)
        {
            var sw = new Stopwatch();
            sw.Start();
            var index = 0;
            const int batch = 5000;
            while (true)
            {
                var tasks = streams.Skip(index).Take(batch).Select(AppendingExecute).ToArray();
                Console.WriteLine($"\t{index}...");
                if (tasks.Length == 0) break;
                await Task.WhenAll(tasks);
                index += batch;
            }
            sw.Stop();
            Console.WriteLine($@"Multithreaded appending test | Overall time {sw.ElapsedMilliseconds}ms - {Math.Round(messagesCount / sw.Elapsed.TotalSeconds, 2, MidpointRounding.AwayFromZero)} event/sec | streams {streams.Count} | avg events per stream {messagesCount / streams.Count} | events {messagesCount}");
        }

        private static async Task AppendingExecute(UncommittedMessages data)
        {
            await store.AppendToStream(data.CorrelationId.ToString(), ExpectedVersion.NoStream, data).NotOnCapturedContext();
        }

        public static async Task MultithreadedReadingTest(List<UncommittedMessages> streams, int messagesCount)
        {
            var sw = new Stopwatch();
            sw.Start();
            var index = 0;
            const int batch = 5000;
            while (true)
            {
                var tasks = streams.Skip(index).Take(batch).Select(ReadingExecute).ToArray();
                Console.WriteLine($"\t{index}...");
                if (tasks.Length == 0) break;
                await Task.WhenAll(tasks);
                index += batch;
            }
            sw.Stop();
            Console.WriteLine($@"Multithreaded reading test | Overall time {sw.ElapsedMilliseconds}ms - {Math.Round(messagesCount / sw.Elapsed.TotalSeconds, 2, MidpointRounding.AwayFromZero)} event/sec | streams {streams.Count} | avg events per stream {messagesCount / streams.Count} | events {messagesCount}");
        }

        private static async Task ReadingExecute(UncommittedMessages data)
        {
            await store.ReadStreamForward(data.CorrelationId.ToString(), 0);
        }
    }
}