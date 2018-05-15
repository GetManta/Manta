using System;
using System.Threading.Tasks;
using Manta.Projections.Construct.TestProjections;
using Manta.Projections.MsSql;
using Manta.Projections.Runner;
using Manta.Sceleton.Logging;

namespace Manta.Projections.Construct
{
    class Program
    {
        private const string connectionString = @"data source=(local); initial catalog = mantabench; Integrated Security = True; Enlist = false;";

        static void Main(string[] args)
        {
            Execute().Wait();
        }

        private static async Task Execute()
        {
            Console.WriteLine("Starting...");
            var projector = new MsSqlProjector(
                    "StaticUniqueProjectorName",
                    connectionString)
                .AddSerializer(new JilSerializer())
                .AddProjections(typeof(TestProjection).Assembly, t => t.Namespace.StartsWith("Manta.Projections.Construct"))
                .AddLogger(new NullLogger())
                //.AddProjectionFactory(new ProjectionFactory(container))
                .OnProjectingError(
                    x =>
                    {
                        Console.WriteLine($"Projecting error at position {x.Envelope.Meta.MessagePosition}[{x.Context.RetryAttempt}] with message: {x.Exception.Message}.");
                    });
            
            await projector.Run(); // Run once and exit when done

            // each projector has own config about fetching limits/timeouts, etc


            var runner = new ProjectorRunner();
            runner.Add(projector);

            runner.Start();

            Console.WriteLine("Done. Press any key...");
            Console.ReadKey();
        }
    }
}
