using System;

namespace Manta.Sceleton.Converters
{
    public interface IUpConverterFactory
    {
        IUpConvertMessage CreateInstanceFor(Type messageType);
    }
}