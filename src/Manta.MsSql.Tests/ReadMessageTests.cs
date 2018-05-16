using System;
using Manta.MsSql.Tests.Infrastructure;
using Manta.Sceleton;
using Xunit;

namespace Manta.MsSql.Tests
{
    public class ReadMessageTests : TestsBase
    {
        public ReadMessageTests(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async void Can_read_message()
        {
            const string expectedContractName = "b";
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            var result = await store.Advanced.ReadMessage(streamName, 2).NotOnCapturedContext();
            Assert.Equal(expectedContractName, result.ContractName);
        }

        [Fact]
        public async void Reading_not_existing_message_returns_null()
        {
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            var result = await store.Advanced.ReadMessage(streamName, 20).NotOnCapturedContext();
            Assert.Null(result);
        }

        [Fact]
        public async void Reading_message_from_not_existing_stream_returns_null()
        {
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            var result = await store.Advanced.ReadMessage(streamName + "_nono", 2).NotOnCapturedContext();
            Assert.Null(result);
        }

        [Fact]
        public async void Reading_message_with_wrong_version_throws()
        {
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            var exception = Record.ExceptionAsync(async () => await store.Advanced.ReadMessage(streamName, ExpectedVersion.Any).NotOnCapturedContext());
            Assert.NotNull(exception);
        }

        private static UncommittedMessages GetUncommitedMessages()
        {
            var payload = new ArraySegment<byte>(new byte[] { 1, 2, 3 });

            return new UncommittedMessages(
                Guid.NewGuid(),
                new[]
                {
                    new MessageRecord(Guid.NewGuid(), "a", payload),
                    new MessageRecord(Guid.NewGuid(), "b", payload),
                    new MessageRecord(Guid.NewGuid(), "a", payload)
                });
        }
    }
}