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
        public async void After_linearization_in_small_batches_head_position_should_be_equal_expected_number()
        {
            const byte expected = 18;
            const string streamName1 = "test-123";
            const string streamName2 = "test-456";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName1, ExpectedVersion.NoStream, data).NotOnCapturedContext();
            data = GetUncommitedMessages();
            await store.AppendToStream(streamName2, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            using (var linearizer = new MsSqlLinearizer(ConnectionString, new NullLogger(), batchSize: 4))
            {
                await linearizer.Run().NotOnCapturedContext();
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
                    new MessageRecord(Guid.NewGuid(), "a", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "b", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "a", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "a", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "b", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "a", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "a", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "b", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "a", new byte[]{ 1, 2, 3 })
                });
        }
    }
}