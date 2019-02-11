using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Manta.Sceleton.Converters
{
    /// <inheritdoc />
    public abstract class UpConverterFactoryBase : IUpConverterInvoker
    {
        private static readonly Type genericConverterType = typeof(IUpConvertMessage<,>);
        private readonly IDictionary<Type, Type> _converters;
        private readonly Dictionary<Type, Func<object, object, object>> _delegates;

        protected UpConverterFactoryBase(params Assembly[] assemblies)
        {
            _converters = new Dictionary<Type, Type>();
            _delegates = new Dictionary<Type, Func<object, object, object>>();

            foreach (var assembly in assemblies)
            {
                foreach (var converterType in assembly.GetTypes().Where(IsConverter))
                {
                    var genericArgs = converterType.GetInterfaces()
                        .First(x => x.IsGenericType && typeof(IUpConvertMessage).IsAssignableFrom(x))
                        .GetGenericArguments();

                    var messageType = genericArgs[0];
                    if (_converters.ContainsKey(messageType)) continue;

                    var outputMessageType = genericArgs[1];

                    _converters.Add(messageType, converterType);
                    _delegates.Add(messageType, GetConvertMethodFunc(converterType, messageType, outputMessageType, genericConverterType));
                }
            }
        }

        protected Type GetConverterType(Type messageType)
        {
            return _converters.TryGetValue(messageType, out var converterType) ? converterType : null;
        }

        private static bool IsConverter(Type type) => typeof(IUpConvertMessage).IsAssignableFrom(type);

        public object Invoke(IUpConvertMessage converter, Type messageType, object message)
        {
            return _delegates[messageType](converter, message);
        }

        private static Func<object, object, object> GetConvertMethodFunc(Type targetType, Type inputMessageType, Type outputMessageType, Type interfaceGenericType)
        {
            var interfaceType = interfaceGenericType.MakeGenericType(inputMessageType, outputMessageType);
            if (!interfaceType.IsAssignableFrom(targetType)) return null;

            var methodInfo = targetType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();
            if (methodInfo == null) return null;

            var target = Expression.Parameter(typeof(object));
            var messageParam = Expression.Parameter(typeof(object));

            var castTarget = Expression.Convert(target, targetType);

            var methodParameters = methodInfo.GetParameters();
            var messageCastParam = Expression.Convert(messageParam, methodParameters.ElementAt(0).ParameterType);

            var body = Expression.Call(castTarget, methodInfo, messageCastParam);

            return Expression.Lambda<Func<object, object, object>>(body, target, messageParam).Compile();
        }
    }
}