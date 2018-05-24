using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Manta.Projections
{
    public class ProjectionDescriptor
    {
        private static readonly Type handlerType = typeof(IProjecting<>);

        internal ProjectionDescriptor(Type projectionType)
        {
            ProjectionType = projectionType;
            ContractName = GetContractName(ProjectionType);
            MessageTypes = FindMessageTypes(ProjectionType);
            Delegates = new Dictionary<Type, Func<object, object, IMetadata, ProjectingContext, Task>>(MessageTypes.Count);

            foreach (var messageType in MessageTypes)
            {
                Delegates.Add(messageType, GetOnMethodFunc(projectionType, messageType, handlerType));
            }
        }

        public Type ProjectionType { get; }
        public string ContractName { get; }
        public HashSet<Type> MessageTypes { get; }
        public long CurrentPosition => Checkpoint?.Position ?? 0;
        public DateTime? DroppedAtUtc => Checkpoint?.DroppedAtUtc;
        public bool IsDropped()
        {
            return DroppedAtUtc != null;
        }

        internal Dictionary<Type, Func<object, object, IMetadata, ProjectingContext, Task>> Delegates { get; }
        internal IProjectionCheckpoint Checkpoint { get; set; }

        internal void Drop()
        {
            Checkpoint.DroppedAtUtc = DateTime.UtcNow;
        }

        internal bool IsProjecting(Type messageType)
        {
            return MessageTypes.Contains(messageType);
        }

        private static HashSet<Type> FindMessageTypes(Type projectionType)
        {
            var interfaces = projectionType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType)
                .ToArray();

            var list = new HashSet<Type>();
            foreach (var i in interfaces)
            {
                var messageTypes = i.GetGenericArguments();
                foreach (var type in messageTypes)
                {
                    list.Add(type);
                }
            }
            return list;
        }

        private static string GetContractName(Type type)
        {
            var attr = type.GetCustomAttribute<DataContractAttribute>();
            return attr == null ? type.FullName : attr.Name;
        }

        private static Func<object, object, IMetadata, ProjectingContext, Task> GetOnMethodFunc(Type targetType, Type messageType, Type interfaceGenericType)
        {
            var interfaceType = interfaceGenericType.MakeGenericType(messageType);
            if (!interfaceType.IsAssignableFrom(targetType)) return null;

            var methodInfo = targetType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();
            if (methodInfo == null) return null; 

            var target = Expression.Parameter(typeof(object));
            var messageParam = Expression.Parameter(typeof(object));
            var metadataParam = Expression.Parameter(typeof(IMetadata));
            var projectingContextParam = Expression.Parameter(typeof(ProjectingContext));

            var castTarget = Expression.Convert(target, targetType);

            var methodParameters = methodInfo.GetParameters();
            var messageCastParam = Expression.Convert(messageParam, methodParameters.ElementAt(0).ParameterType);

            var body = Expression.Call(castTarget, methodInfo, messageCastParam, metadataParam, projectingContextParam);

            return Expression.Lambda<Func<object, object, IMetadata, ProjectingContext, Task>>(body, target, messageParam, metadataParam, projectingContextParam).Compile();
        }
    }
}