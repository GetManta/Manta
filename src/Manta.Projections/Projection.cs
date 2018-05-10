using System;
using System.Threading.Tasks;

namespace Manta.Projections
{
    public abstract class Projection
    {
        public virtual Task Create() { return Task.CompletedTask; }
        public virtual Task Destroy() { return Task.CompletedTask; }
        public virtual ExceptionResolution DefaultExceptionResolution => ExceptionResolution.Retry;
    }

    public enum ExceptionResolution : byte
    {
        Ignore = 0,
        Retry = 1,
        Drop = 2
    }

    public interface IProject<in TMessage>
    {
        Task On(TMessage m, Metadata meta, ProjectingContext context);
    }

    public class ProjectingContext
    {
        public byte MaxRetryAttempts { get; private set; }
        public byte RetryAttempts { get; private set; }

        public void Retry()
        {
            Solution = ExceptionResolution.Retry;
        }

        public void Drop()
        {
            Solution = ExceptionResolution.Drop;
        }

        public void Ignore()
        {
            Solution = ExceptionResolution.Ignore;
        }

        internal ExceptionResolution Solution { get; private set; }
    }

    public class ProjectionException : Exception
    {

    }
}