using System;
using Manta.MsSql.Tests.Infrastructure;
using Xunit;

namespace Manta.MsSql.Tests
{
    public class AppendToStreamAnyVersionTests : TestsBase
    {
        public AppendToStreamAnyVersionTests(LocalDbFixture fixture) : base(fixture) { }

        [Fact]
        public async void appending_messages_with_any_version_not_throws()
        {
            const string streamName = "test-123";

            var data = new UncommittedMessages(
                Guid.NewGuid(),
                new []
                {
                    new MessageRecord(Guid.NewGuid(), 0, new byte[]{ 1, 2, 3 }),
                    new MessageRecord(Guid.NewGuid(), 1, new byte[]{ 1, 2, 3 })
                });

            var store = await GetMessageStore();
            var exception = await Record.ExceptionAsync(() => store.AppendToStream(streamName, ExpectedVersion.Any, data));

            Assert.Null(exception);
        }
    }
}