using System;
using Manta.MsSql.Tests.Infrastructure;
using Manta.Sceleton;
using Xunit;
// ReSharper disable PossibleNullReferenceException

namespace Manta.MsSql.Tests
{
    public class AppendToStreamNoStreamTests : TestsBase
    {
        public AppendToStreamNoStreamTests(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async void Appending_messages_to_nonexisted_stream_with_nostream_expected_version_not_throws()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();

            var exception = await Record.ExceptionAsync(async () => await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext());

            Assert.Null(exception);
        }

        [Fact]
        public async void Appending_messages_to_existed_stream_with_nostream_expected_version_throws()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();
            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            data = GetUncommitedMessages();

            await Assert.ThrowsAsync<WrongExpectedVersionException>(async () => await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext());
        }

        [Fact]
        public async void Appending_the_same_messages_to_existed_stream_with_nostream_expected_version_should_be_idempotent()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();

            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data);

            var exception = await Record.ExceptionAsync(async () => await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext());

            Assert.Null(exception);
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