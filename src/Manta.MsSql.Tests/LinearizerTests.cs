using System;
using Manta.MsSql.Tests.Infrastructure;
using Manta.Sceleton;
using Manta.Sceleton.Logging;
using Xunit;

namespace Manta.MsSql.Tests
{
    public class LinearizerTests : TestsBase
    {
        public LinearizerTests(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async void After_linearization_head_position_is_equal_expected_number()
        {
            const byte expected = 3;
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            using (var linearizer = new MsSqlLinearizer(ConnectionString, new NullLogger(), batchSize: 1))
            {
                await linearizer.RunNow();
            }

            var head = await store.Advanced.ReadHeadMessagePosition().NotOnCapturedContext();
            
            Assert.Equal(expected, head);
        }

        private static UncommittedMessages GetUncommitedMessages()
        {
            return new UncommittedMessages(
                Guid.NewGuid(),
                new[]
                {
                    new MessageRecord(Guid.NewGuid(), 0, new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), 1, new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), 0, new byte[]{ 1, 2, 3 })
                });
        }
    }
}