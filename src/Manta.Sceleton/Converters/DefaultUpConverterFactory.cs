using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Manta.Sceleton.Converters
{
    public class DefaultUpConverterFactory : IUpConverterFactory
    {
        private readonly IDictionary<Type, Type> _converters;

        public DefaultUpConverterFactory(params Assembly[] assemblies)
        {
            _converters = new Dictionary<Type, Type>();
            foreach (var assembly in assemblies)
            {
                foreach (var converterType in assembly.GetTypes().Where(IsConverter))
                {
                    var messageType =
                        converterType.GetInterfaces()
                            .First(x => x.IsGenericType && typeof(IUpConvertMessage).IsAssignableFrom(x))
                            .GetGenericArguments()
                            .First();

                    if (_converters.ContainsKey(messageType)) continue;

                    _converters.Add(messageType, converterType);
                }
            }
        }

        private static bool IsConverter(Type type)
        {
            return typeof(IUpConvertMessage).IsAssignableFrom(type);
        }

        public IUpConvertMessage CreateInstanceFor(Type messageType)
        {
            return (!_converters.TryGetValue(messageType, out var converter)
                ? null
                : Activator.CreateInstance(converter)) as IUpConvertMessage;
        }
    }
}