using System;
using System.Reflection;

namespace Manta.Sceleton.Converters
{
    public class DefaultUpConverterFactory : UpConverterFactoryBase, IUpConverterFactory
    {
        public DefaultUpConverterFactory(params Assembly[] assemblies) : base(assemblies) { }

        public IUpConvertMessage CreateInstanceFor(Type messageType)
        {
            return (!Converters.TryGetValue(messageType, out var converter)
                ? null
                : Activator.CreateInstance(converter)) as IUpConvertMessage;
        }
    }
}