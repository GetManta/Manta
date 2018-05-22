using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Manta.Projections
{
    public class ProjectionDescriptor : IProjectionDescriptor
    {
        private static readonly Type handlerType = typeof(IProjecting<>);

        internal ProjectionDescriptor(Type projectionType)
        {
            ProjectionType = projectionType;
            ContractName = GetContractName(ProjectionType);
            MessageTypes = FindMessageTypes(ProjectionType);
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

        public Type ProjectionType { get; }
        public string ContractName { get; }
        public HashSet<Type> MessageTypes { get; }
        public long CurrentPosition => Checkpoint?.Position ?? 0;
        public DateTime? DroppedAtUtc => Checkpoint?.DroppedAtUtc;
        public bool IsDropped()
        {
            return DroppedAtUtc != null;
        }

        internal IProjectionCheckpoint Checkpoint { get; set; }

        internal void Drop()
        {
            Checkpoint.DroppedAtUtc = DateTime.UtcNow;
        }

        internal bool IsProjecting(Type messageType)
        {
            return MessageTypes.Contains(messageType);
        }
    }
}