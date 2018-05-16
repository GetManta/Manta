using System;

namespace Manta.Projections
{
    public class DispatchingResult
    {
        public int EnvelopesCount { get; }
        public long ElapsedMilliseconds { get; }
        public bool AnyDispatched { get; }
        public Exception Exception { get; }

        internal DispatchingResult(Exception exception, int envelopesCount, long elapsedMilliseconds, bool anyDispatched)
            : this(envelopesCount, elapsedMilliseconds, anyDispatched)
        {
            Exception = exception;
        }

        internal DispatchingResult(int envelopesCount, long swElapsedMilliseconds, bool anyDispatched)
        {
            EnvelopesCount = envelopesCount;
            ElapsedMilliseconds = swElapsedMilliseconds;
            AnyDispatched = anyDispatched;
        }
    }
}