using System;

namespace Manta.Projections
{
    public interface IProjectionFactory
    {
        Projection CreateProjectionInstance(Type projectionType);
    }
}