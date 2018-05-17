using System;
using System.Collections.Generic;

namespace Manta.Projections
{
    internal class ActivatorProjectionFactory : IProjectionFactory
    {
        private readonly Dictionary<Type, Projection> _cache;

        public ActivatorProjectionFactory()
        {
            _cache = new Dictionary<Type, Projection>(10);
        }

        public Projection CreateProjectionInstance(Type projectionType)
        {
            if (_cache.TryGetValue(projectionType, out var projection)) return projection;

            projection = Activator.CreateInstance(projectionType) as Projection;
            _cache.Add(projectionType, projection);
            return projection;
        }
    }
}