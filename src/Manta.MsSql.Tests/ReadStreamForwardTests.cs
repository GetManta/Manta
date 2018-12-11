using System;
using System.Linq;
using Manta.MsSql.Tests.Infrastructure;
using Manta.Sceleton;
using Xunit;

namespace Manta.MsSql.Tests
{
    public class ReadStreamForwardTests : TestsBase
    {
        [Fact]
        public async void Can_read_forward_all_messages()
        {
            const int expectedVersion = 3;
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            var result = await store.ReadStreamForward(streamName, ExpectedVersion.NoStream).NotOnCapturedContext();
            Assert.Equal(expectedVersion, result.Messages.Last().Version);
        }

        [Fact]
        public async void Can_read_forward_from_given_version()
        {
            const int expectedCount = 2;
            const int expectedVersion = 3;
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            var result = await store.ReadStreamForward(streamName, 2).NotOnCapturedContext();
            Assert.Equal(expectedVersion, result.Messages.Last().Version);
            Assert.Equal(expectedCount, result.Messages.Length);
        }

        [Fact]
        public async void Can_read_forward_from_empty_stream()
        {
            const byte expectedVersion = 3;
            const string streamName = "test-1234";
            var store = await GetMessageStore();

            var result = await store.ReadStreamForward(streamName, expectedVersion).NotOnCapturedContext();
            Assert.Empty(result.Messages);
            Assert.True(result.IsStreamNotFound());
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