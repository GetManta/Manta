using System;

namespace Manta.Sceleton.Converters
{
    public interface IUpConverterExecutor
    {
        object Execute(IUpConvertMessage converter, Type messageType, object message);
    }
}