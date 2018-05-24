using System;

namespace Manta.Sceleton.Converters
{
    public interface IUpConverterFactory : IUpConverterInvoker
    {
        IUpConvertMessage CreateInstanceFor(Type messageType);
    }
}