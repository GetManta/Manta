using Manta.Sceleton.Converters;

namespace Manta.Sceleton.Tests.Fakes
{
    public class FakeUpConverter23 : IUpConvertMessage<FakeContract2, FakeContract3>
    {
        public FakeContract3 Convert(FakeContract2 sourceEvent)
        {
            return new FakeContract3();
        }
    }
}