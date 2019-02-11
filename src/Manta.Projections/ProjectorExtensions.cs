using System;

namespace Manta.Projections
{
    internal static class ProjectorExtensions
    {
        public static long[] GenerateRanges(this Projector p, long min, long max, long range)
        {
            if (min < 0) throw new ArgumentException("Min value must be greater or equal zero.", nameof(min));
            if (max < 0) throw new ArgumentException("Max value must be greater or equal zero.", nameof(max));
            if (max < min) throw new ArgumentException("Max value must be greater or equal min value.", nameof(max));
            if (range < 1) throw new ArgumentException("Range value must be greater or equal zero.", nameof(range));
            
            var ranges = new long[max - min + 1];
            long z = 0;
            for (var i = min; i <= max; i+=range)
            {
                ranges[z++] = i;
            }

            return ranges;
        }
    }
}