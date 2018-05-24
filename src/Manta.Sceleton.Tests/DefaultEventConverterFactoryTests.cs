using Manta.Sceleton.Converters;
using Manta.Sceleton.Tests.Fakes;
using Xunit;

namespace Manta.Sceleton.Tests
{
    public class DefaultEventConverterFactoryTests
    {
        [Fact]
        public void Creating_converter_instance_for_registered_message_converter_returns_converter_object()
        {
            var fakeContractType = typeof(FakeContract1);
            var sut = new DefaultUpConverterFactory(fakeContractType.Assembly);

            var converter = sut.CreateInstanceFor(fakeContractType);
            Assert.NotNull(converter);
        }

        [Fact]
        public void Converting_from_FakeContract1_to_FakeContract2_succeded()
        {
            var fakeContractType = typeof(FakeContract1);
            var msg1 = new FakeContract1();
            var sut = new DefaultUpConverterFactory(fakeContractType.Assembly);

            var converter = sut.CreateInstanceFor(fakeContractType);
            var msg2 = sut.Invoke(converter, fakeContractType, msg1);
            Assert.NotNull(msg2 as FakeContract2);
        }

        [Fact]
        public void Creating_converter_instance_for_not_registered_event_converter_returns_null()
        {
            var fakeContractType = typeof(FakeContract1);
            var sut = new DefaultUpConverterFactory(fakeContractType.Assembly);

            var converter = sut.CreateInstanceFor(typeof(FakeContract3));
            Assert.Null(converter);
        }
    }
}
