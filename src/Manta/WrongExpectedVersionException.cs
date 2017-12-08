﻿using System;

namespace Manta
{
    public class WrongExpectedVersionException : Exception
    {
        public int ConflictedContract { get; private set; }
        public int ConflictedVersion { get; private set; }

        public WrongExpectedVersionException(string message, Exception inner = null)
            : base(message, inner) { }

        public WrongExpectedVersionException(string message, int conflictedVersion, int conflictedContract, Exception inner = null)
            : base(message, inner)
        {
            ConflictedVersion = conflictedVersion;
            ConflictedContract = conflictedContract;
        }
    }
}