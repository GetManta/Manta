using System;
using System.Linq;
using Manta.MsSql.Tests.Infrastructure;
using Manta.Sceleton;
using Xunit;

namespace Manta.MsSql.Tests
{
    public class ReadStreamForwardTests : TestsBase
    {
        public ReadStreamForwardTests(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async void Can_read_forward_all_messages()
        {
            const byte expectedVersion = 3;
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
            const byte expectedCount = 2;
            const byte expectedVersion = 3;
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
            const byte expectedCount = 0;
            const byte expectedVersion = 3;
            const string streamName = "test-1234";
            var store = await GetMessageStore();

            var result = await store.ReadStreamForward(streamName, expectedVersion).NotOnCapturedContext();
            Assert.Equal(expectedCount, result.Messages.Length);
            Assert.True(result.IsStreamNotFound());
        }

        private static UncommittedMessages GetUncommitedMessages()
        {
            return new UncommittedMessages(
                Guid.NewGuid(),
                new[]
                {
                    new MessageRecord(Guid.NewGuid(), "a", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "b", new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), "a", new byte[]{ 1, 2, 3 })
                });
        }
    }
}