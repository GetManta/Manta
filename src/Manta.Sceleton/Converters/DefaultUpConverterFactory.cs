using System;
using System.Reflection;

namespace Manta.Sceleton.Converters
{
    public class DefaultUpConverterFactory : UpConverterFactoryBase, IUpConverterFactory
    {
        public DefaultUpConverterFactory(params Assembly[] assemblies) : base(assemblies) { }

        public IUpConvertMessage CreateInstanceFor(Type messageType)
        {
            var t = GetConverterType(messageType);
            return t == null ? null : Activator.CreateInstance(t) as IUpConvertMessage;
        }
    }
}