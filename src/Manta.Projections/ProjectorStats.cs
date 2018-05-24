using System;
using System.Collections.Generic;
using System.Linq;

namespace Manta.Projections
{
    public struct ProjectorStats
    {
        internal ProjectorStats(List<DispatchingResult> results)
        {
            Results = results;
            TotalMessages = results.Sum(x => x.EnvelopesCount);
            if (TotalMessages > 0)
            {
                TotalSeconds = (double)results.Sum(x => x.ElapsedMilliseconds) / 1000;
                AveragePerSecond = Math.Round(TotalMessages / TotalSeconds, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                TotalSeconds = 0;
                AveragePerSecond = 0;
            }
        }

        public IEnumerable<DispatchingResult> Results { get; }
        public double AveragePerSecond { get; }
        public double TotalSeconds { get; }
        public int TotalMessages { get; }

        public override string ToString()
        {
            return $"Total time {TotalSeconds}sec | Processed {TotalMessages} messages | Average processing {AveragePerSecond}/sec";
        }
    }
}