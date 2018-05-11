using System;
using System.Threading.Tasks;
using Manta.Projections.Construct.TestProjections;
using Manta.Projections.MsSql;

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
            var projector = new Projector(
                "StaticUniqueProjectorName",
                new MsSqlDataSource(connectionString),
                new MsSqlCheckpointRepository(connectionString));

            // each projector has own config about fetching limits/timeouts, etc

            // projector.AddLogger(new NLogLogger());
            // projector.AddProjectionFactory(new ProjectionFactory(container));
            // projector.AddProjection<TestProjection>();

            projector.AddProjections(typeof(TestProjection).Assembly, t => t.Namespace.StartsWith("Manta.Projections.Construct"));

            projector.OnProjectingError(
                (d, env, ctx, exc) =>
                {
                    Console.WriteLine("Projecting error " + exc.Message);
                });

            await projector.Run(); // Run once and exit when done

            // projector.Start(settings); // Start running periodically
            // projector.Stop(); // Stop running periodically

            Console.WriteLine("Done. Press any key...");
            Console.ReadKey();
        }
    }
}
