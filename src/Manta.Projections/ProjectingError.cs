using System;

namespace Manta.Projections
{
    public class ProjectingError
    {
        public ProjectionDescriptor Descriptor { get; }
        public MessageEnvelope Envelope { get; }
        public ProjectingContext Context { get; }
        public Exception Exception { get; }

        internal ProjectingError(ProjectionDescriptor descriptor, MessageEnvelope envelope, ProjectingContext context, Exception exception)
        {
            Descriptor = descriptor;
            Envelope = envelope;
            Context = context;
            Exception = exception;
        }
    }
}