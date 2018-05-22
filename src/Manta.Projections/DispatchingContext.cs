using System.Collections.Generic;

namespace Manta.Projections
{
    internal class DispatchingContext
    {
        public ProjectionDescriptor Descriptor { get; }
        public List<MessageEnvelope> Envelopes { get; }

        public DispatchingContext(ProjectionDescriptor descriptor, List<MessageEnvelope> envelopes)
        {
            Descriptor = descriptor;
            Envelopes = envelopes;
        }
    }
}