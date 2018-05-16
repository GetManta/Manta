using Manta.Sceleton.Converters;

namespace Manta.Sceleton.Tests.Fakes
{
    public class FakeUpConverter12 : IUpConvertMessage<FakeContract1, FakeContract2>
    {
        public FakeContract2 Convert(FakeContract1 sourceEvent)
        {
            return new FakeContract2();
        }
    }
}