using System;
using Manta.MsSql.Tests.Infrastructure;
using Xunit;
// ReSharper disable PossibleNullReferenceException

namespace Manta.MsSql.Tests
{
    public class AppendToStreamExpectedVersionTests : TestsBase
    {
        public AppendToStreamExpectedVersionTests(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async void Appending_the_same_messages_to_existed_stream_with_proper_expected_version_should_be_idempotent()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();

            await store.AppendToStream(streamName, ExpectedVersion.Any, data);
            var exception = await Record.ExceptionAsync(() => store.AppendToStream(streamName, 3, data));

            Assert.Null(exception);
        }

        [Fact]
        public async void Appending_messages_to_existed_stream_with_proper_expected_version_not_throws()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();

            await store.AppendToStream(streamName, ExpectedVersion.Any, data);

            data = GetUncommitedMessages();

            var exception = await Record.ExceptionAsync(() => store.AppendToStream(streamName, 3, data));

            Assert.Null(exception);
        }

        [Fact]
        public async void Appending_messages_to_existed_stream_with_to_high_expected_version_throws()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();

            await store.AppendToStream(streamName, ExpectedVersion.Any, data);

            data = GetUncommitedMessages();

            var exception = await Record.ExceptionAsync(() => store.AppendToStream(streamName, 4, data));

            Assert.NotNull(exception);
        }

        [Fact]
        public async void Appending_messages_to_existed_stream_with_to_low_expected_version_throws()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();

            await store.AppendToStream(streamName, ExpectedVersion.Any, data);

            data = GetUncommitedMessages();

            var exception = await Record.ExceptionAsync(() => store.AppendToStream(streamName, 2, data));

            Assert.NotNull(exception);
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