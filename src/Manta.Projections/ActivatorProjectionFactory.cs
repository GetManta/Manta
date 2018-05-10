using System;

namespace Manta.Projections
{
    internal class ActivatorProjectionFactory : IProjectionFactory
    {
        public Projection CreateProjectionInstance(Type projectionType)
        {
            return Activator.CreateInstance(projectionType) as Projection;
        }
    }
}