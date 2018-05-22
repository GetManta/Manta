namespace Manta.Projections
{
    public class ProjectingContext
    {
        internal ProjectingContext(byte maxProjectingRetries, long startingBatchAtPosition)
        {
            MaxProjectingRetries = maxProjectingRetries;
            StartingBatchAtPosition = startingBatchAtPosition;
            Reset();
        }

        public byte MaxProjectingRetries { get; }
        public long StartingBatchAtPosition { get; }
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

        internal void Reset()
        {
            ExceptionSolution = ExceptionSolutions.Retry;
            RetryAttempt = 1;
        }
    }
}