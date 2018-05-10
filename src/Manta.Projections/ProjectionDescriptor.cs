using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Manta.Projections
{
    public class ProjectionDescriptor
    {
        public ProjectionDescriptor(Type projectionType)
        {
            ProjectionType = projectionType;
            ContractName = GetContractName(ProjectionType);
        }

        private static string GetContractName(Type type)
        {
            var attr = type.GetCustomAttribute<DataContractAttribute>();
            return attr == null ? type.FullName : attr.Name;
        }

        public Type ProjectionType { get; }
        public string ContractName { get; }

        public IProjectionCheckpoint Checkpoint { get; set; }
    }
}