using System;
using System.Linq;
using Manta.MsSql;
using Manta.Projections.MsSql.Tests.Infrastructure;
using Manta.Projections.MsSql.Tests.TestProjections;
using Manta.Sceleton;
using Manta.Sceleton.Logging;
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
            var projector = await GetProjector(c => c.AddProjections(typeof(TestContracts).Assembly));

            var exception = await Record.ExceptionAsync(
                async () =>
                {
                    await projector.Run();
                });

            Assert.Null(exception);
        }

        [Fact]
        public async void after_running_projector_currentposition_on_projection_is_equal_to_max_messageposition()
        {
            var store = await GetMessageStore();
            const string streamName = "test-321";
            var data = GetUncommitedMessages();

            await store.AppendToStream(streamName, ExpectedVersion.Any, data).NotOnCapturedContext();
            using (var linearizer = new MsSqlLinearizer(ConnectionString, new NullLogger()))
            {
                await linearizer.Run().NotOnCapturedContext();
            }

            var head = await store.Advanced.ReadHeadMessagePosition();

            var projector = await GetProjector(c => c.AddProjection<TestProjection>());
            await projector.Run();

            Assert.Equal(data.Messages.Length, projector.GetProjections().Max(x => x.CurrentPosition));
        }

        private static UncommittedMessages GetUncommitedMessages()
        {
            var serializer = new JilSerializer();
            var payload = serializer.Serialize(new TestContracts.MessageOne { ID = 1, Name = "Test data" });
            var contractName = TestContracts.GetContractNameByType(typeof(TestContracts.MessageOne));

            return new UncommittedMessages(
                Guid.NewGuid(),
                new[]
                {
                    new MessageRecord(Guid.NewGuid(), contractName, payload),
                    new MessageRecord(Guid.NewGuid(), contractName, payload),
                    new MessageRecord(Guid.NewGuid(), contractName, payload)
                });
        }
    }
}