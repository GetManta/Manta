﻿using System;

namespace Manta.Projections
{
    public class DispatchingResult
    {
        private DispatchingResult(ProjectionDescriptor descriptor, Statuses status)
        {
            Descriptor = descriptor;
            Status = status;
        }

        private DispatchingResult(ProjectionDescriptor descriptor, Statuses status, int envelopesCount, long elapsedMilliseconds, bool anyDispatched, Exception exception = null)
            : this(descriptor, status)
        {
            EnvelopesCount = envelopesCount;
            ElapsedMilliseconds = elapsedMilliseconds;
            AnyDispatched = anyDispatched;
            Exception = exception;
        }

        public ProjectionDescriptor Descriptor { get; }
        public Statuses Status { get; }

        public int EnvelopesCount { get; }
        public long ElapsedMilliseconds { get; }
        public bool AnyDispatched { get; }
        public Exception Exception { get; }

        public bool HaveCaughtException()
        {
            return Exception != null;
        }

        internal static DispatchingResult StillDropped(ProjectionDescriptor descriptor) => new DispatchingResult(descriptor, Statuses.StillDropped);

        internal static DispatchingResult Dispatched(ProjectionDescriptor descriptor, int envelopesCount, long elapsedMilliseconds, bool anyDispatched)
        {
            return new DispatchingResult(descriptor, Statuses.Dispatched, envelopesCount, elapsedMilliseconds, anyDispatched);
        }

        internal static DispatchingResult DroppedOnException(ProjectionDescriptor descriptor, int envelopesCount, long elapsedMilliseconds, Exception exception)
        {
            return new DispatchingResult(descriptor, Statuses.DroppedOnException, envelopesCount, elapsedMilliseconds, false, exception);
        }

        public enum Statuses : byte
        {
            Dispatched = 0,
            StillDropped = 1,
            DroppedOnException = 2
        }
    }
}