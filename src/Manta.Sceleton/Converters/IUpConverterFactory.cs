using System;

namespace Manta.Sceleton.Converters
{
    public interface IUpConverterFactory
    {
        IUpConvertMessage CreateInstance(Type type);
    }
}