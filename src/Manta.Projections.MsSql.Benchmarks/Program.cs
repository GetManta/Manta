using System;
using System.Threading.Tasks;
using Benchmarks.Shared;
using Manta.Projections.MsSql.Benchmarks.TestProjections;
using Manta.Projections.MsSql.Installer;
using Manta.Sceleton.Logging;

namespace Manta.Projections.MsSql.Benchmarks
{
    internal class Program
    {
        private const string connectionString = @"data source=(local); initial catalog = mantabench; Integrated Security = True; Enlist = false;";

        private static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            Install().Wait();

            var projector = new MsSqlProjector("StaticUniqueProjectorName", connectionString, new JilSerializer())
                .AddProjections(typeof(TestProjection).Assembly, t => t.Namespace.StartsWith("Manta.Projections.MsSql.Benchmarks.TestProjections"))
                .AddLogger(new NullLogger())
                .WithStatistics(s => Console.WriteLine(s.ToString()))
                .OnProjectingError(
                    x =>
                    {
                        Console.WriteLine($"Projecting error at position {x.Envelope.Meta.MessagePosition}[Retry: {x.Context.RetryAttempt}] with message: {x.InnerException.Message}.");
                    });

            // ExecuteOnce(projector).Wait();

            ExecuteRunner(projector);

            Console.WriteLine("\r\nPress any key to exit...");
            Console.ReadKey();
        }

        private static async Task Install()
        {
            var installer = new MsSqlProjectorsInstaller(connectionString);
            await installer.Execute();
        }

        private static async Task ExecuteOnce(ProjectorBase projector)
        {
            await projector.Run(); // Run once and exit when done
        }

        private static void ExecuteRunner(ProjectorBase projector)
        {
            // each projector runner has own config about fetching limits / timeouts, etc
            using (var runner = new ProjectorRunner())
            {
                runner.Add(projector);

                runner.Start();

                Console.WriteLine("\r\nPress any key to stop runner...");
                Console.ReadKey();

                runner.Stop();
            }
        }
    }
}
