using System;

namespace Manta.Projections
{
    public class ProjectingException : Exception
    {
        public ProjectionDescriptor Descriptor { get; }
        public MessageEnvelope Envelope { get; }
        public ProjectingContext Context { get; }

        public ProjectingException(ProjectionDescriptor descriptor, MessageEnvelope envelope, ProjectingContext context, Exception exception)
            : base($"Projecting '{descriptor.ContractName}' with message '{envelope.Meta.MessageContractName}' exception.", exception)
        {
            Descriptor = descriptor;
            Envelope = envelope;
            Context = context;
        }
    }
}