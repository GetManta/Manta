﻿using System;
using System.Collections.Generic;

namespace Manta.Projections
{
    internal class TimeoutCalculator
    {
        private const short minTimeoutValue = 20;
        private const short maxTimeoutLevel = 500;
        private static readonly Dictionary<short, short> timeoutLevels = new Dictionary<short, short>
        {
            // count | ms
            { maxTimeoutLevel, 1000 },
            { 300, 500 },
            { 400, 250 },
            { 500, 100 },
            { 500, 60 },
            { 200, 40 },
            { 0, minTimeoutValue }
        };

        private short _notDispatchingCounter;
        private readonly TimeSpan _staticTimeout;

        public TimeoutCalculator(TimeSpan staticTimeout)
        {
            _staticTimeout = staticTimeout;
        }

        public double CalculateNext(bool anyDispatched = true)
        {
            if (_staticTimeout != TimeSpan.Zero)
            {
                _notDispatchingCounter = 0;
                return anyDispatched ? minTimeoutValue : _staticTimeout.TotalMilliseconds;
            }

            if (!anyDispatched)
            {
                if (_notDispatchingCounter >= maxTimeoutLevel) return timeoutLevels[maxTimeoutLevel];

                var levelValue = minTimeoutValue;
                foreach (var level in timeoutLevels)
                {
                    if (_notDispatchingCounter <= level.Key) continue;
                    levelValue = level.Value;
                    break;
                }
                _notDispatchingCounter++;
                return levelValue;
            }
            _notDispatchingCounter = 0;
            return minTimeoutValue;
        }
    }
}