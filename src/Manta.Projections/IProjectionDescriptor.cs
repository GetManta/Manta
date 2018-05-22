using System;
using System.Collections.Generic;

namespace Manta.Projections
{
    public interface IProjectionDescriptor
    {
        Type ProjectionType { get; }
        string ContractName { get; }
        HashSet<Type> MessageTypes { get; }
        long CurrentPosition { get; }
        bool IsDropped();
        DateTime? DroppedAtUtc { get; }
    }
}