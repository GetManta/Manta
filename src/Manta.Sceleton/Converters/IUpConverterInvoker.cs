using System;

namespace Manta.Sceleton.Converters
{
    public interface IUpConverterInvoker
    {
        object Invoke(IUpConvertMessage converter, Type messageType, object message);
    }
}