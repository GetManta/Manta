using System;
using Manta.MsSql.Tests.Infrastructure;
using Manta.Sceleton;
using Xunit;
// ReSharper disable PossibleNullReferenceException

namespace Manta.MsSql.Tests
{
    public class TruncateStreamToCreationDate : TestsBase
    {
        [Fact]
        public async void Truncate_stream_with_expected_version_doesnt_throws()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();

            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            var exception = await Record.ExceptionAsync(async () => await store.Advanced.TruncateStream(
                stream: streamName,
                expectedVersion: data.Messages.Length,
                toCreationDate: DateTime.UtcNow.AddSeconds(-5)).NotOnCapturedContext());

            Assert.Null(exception);
        }

        [Fact]
        public async void Truncating_stream_when_creationDate_is_greater_than_expected_version_throws()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";
            var data = GetUncommitedMessages();

            await store.AppendToStream(streamName, ExpectedVersion.NoStream, data).NotOnCapturedContext();

            var exception = await Record.ExceptionAsync(async () => await store.Advanced.TruncateStream(
                stream: streamName,
                expectedVersion: 1,
                toCreationDate: DateTime.UtcNow.AddSeconds(5)).NotOnCapturedContext());

            Assert.NotNull(exception);
        }

        [Fact]
        public async void Truncating_stream_with_NoStream_expected_version_throws_InvalidOperationException()
        {
            var store = await GetMessageStore();
            const string streamName = "test-123";

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                {
                    await store.Advanced.TruncateStream(
                        stream: streamName,
                        expectedVersion: ExpectedVersion.NoStream,
                        toCreationDate: DateTime.UtcNow.AddSeconds(-5)).NotOnCapturedContext();
                });
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