using System.Collections.Generic;

namespace Manta.Projections
{
    internal static class ProjectorExtensions
    {
        public static IEnumerable<long> GenerateRanges(this Projector p, long min, long max, long range)
        {
            for (var i = min; i < max; i += range) yield return i;
        }
    }
}