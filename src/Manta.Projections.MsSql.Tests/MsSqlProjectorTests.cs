using Manta.Projections.MsSql.Tests.Infrastructure;
using Xunit;
// ReSharper disable PossibleNullReferenceException

namespace Manta.Projections.MsSql.Tests
{
    public class MsSqlProjectorTests : TestsBase
    {
        public MsSqlProjectorTests(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async void running_projector_not_throws()
        {
            var projector = await GetProjector(c => c.AddProjections(typeof(TestProjections.TestContracts).Assembly));

            var exception = await Record.ExceptionAsync(
                async () =>
                {
                    await projector.Run();
                });

            Assert.Null(exception);
        }
    }
}