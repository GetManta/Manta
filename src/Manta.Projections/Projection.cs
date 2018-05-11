using System;
using System.Threading.Tasks;

namespace Manta.Projections
{
    public abstract class Projection
    {
        public virtual Task Create() { return Task.CompletedTask; }
        public virtual Task Destroy() { return Task.CompletedTask; }
    }

    public enum ExceptionSolutions : byte
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
        internal ProjectingContext(byte maxProjectingRetries)
        {
            MaxProjectingRetries = maxProjectingRetries;
            ExceptionSolution = ExceptionSolutions.Retry;
            RetryAttempt = 1;
        }

        public byte MaxProjectingRetries { get; }
        public byte RetryAttempt { get; private set; }

        internal void NextRetry()
        {
            RetryAttempt++;
        }

        public void Retry()
        {
            ExceptionSolution = ExceptionSolutions.Retry;
        }

        public void Drop()
        {
            ExceptionSolution = ExceptionSolutions.Drop;
        }

        public void Ignore()
        {
            ExceptionSolution = ExceptionSolutions.Ignore;
        }

        internal ExceptionSolutions ExceptionSolution { get; private set; }

        public bool CanRetry()
        {
            return RetryAttempt < MaxProjectingRetries;
        }
    }

    public class ProjectionException : Exception
    {

    }
}