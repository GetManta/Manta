using System;

namespace Manta.Projections
{
    internal class DispatchingResult
    {
        public int EnvelopesCount { get; }
        public long ElapsedMilliseconds { get; }
        public bool AnyDispatched { get; }
        public Exception Exception { get; }

        public DispatchingResult(Exception exception, int envelopesCount, long elapsedMilliseconds, bool anyDispatched)
            : this(envelopesCount, elapsedMilliseconds, anyDispatched)
        {
            Exception = exception;
        }

        public DispatchingResult(int envelopesCount, long swElapsedMilliseconds, bool anyDispatched)
        {
            EnvelopesCount = envelopesCount;
            ElapsedMilliseconds = swElapsedMilliseconds;
            AnyDispatched = anyDispatched;
        }
    }
}