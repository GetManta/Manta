using System;
using Manta.MsSql.Tests.Infrastructure;
using Xunit;
// ReSharper disable PossibleNullReferenceException

namespace Manta.MsSql.Tests
{
    public class ReadStreamForward : TestsBase
    {
        public ReadStreamForward(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async void Can_read_forward_all_messages()
        {
            const byte expectedVersion = 3;
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data);

            var result = await store.ReadStreamForward(streamName, ExpectedVersion.NoStream);
            Assert.Equal(expectedVersion, result.CommittedVersion);
        }

        [Fact]
        public async void Can_read_forward_from_given_version()
        {
            const byte expectedCount = 2;
            const byte expectedVersion = 3;
            const string streamName = "test-123";
            var store = await GetMessageStore();
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data);

            var result = await store.ReadStreamForward(streamName, 2);
            Assert.Equal(expectedVersion, result.CommittedVersion);
            Assert.Equal(expectedCount, result.Messages.Length);
        }

        [Fact]
        public async void Can_read_forward_from_empty_stream()
        {
            const byte expectedCount = 0;
            const byte expectedVersion = 3;
            const string streamName = "test-1234";
            var store = await GetMessageStore();

            var result = await store.ReadStreamForward(streamName, expectedVersion);
            Assert.Equal(expectedCount, result.Messages.Length);
            Assert.True(result.IsStreamNotFound());
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