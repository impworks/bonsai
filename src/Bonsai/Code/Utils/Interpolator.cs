using System;

namespace Bonsai.Code.Utils
{
    /// <summary>
    /// Helper class for interpolating values using linear intervals.
    /// </summary>
    public static class Interpolator
    {
        /// <summary>
        /// Returns the value using mapped intervals.
        /// </summary>
        public static int MapValue(int value, params IntervalMap[] intervals)
        {
            foreach (var interval in intervals)
                if (interval.Includes(value))
                    return interval.Map(value);

            throw new ArgumentException($"Value '{value}' does not match any ranges.");
        }
    }

    /// <summary>
    /// Details about an interpolation interval.
    /// </summary>
    public struct IntervalMap
    {
        public IntervalMap(int srcFrom, int srcTo, int destFrom, int destTo)
        {
            if (srcFrom > srcTo)
                throw new ArgumentException();

            SourceFrom = srcFrom;
            SourceTo = srcTo;
            DestFrom = destFrom;
            DestTo = destTo;
        }

        public readonly int SourceFrom;
        public readonly int SourceTo;

        public readonly int DestFrom;
        public readonly int DestTo;

        /// <summary>
        /// Checks if the value is in range.
        /// </summary>
        public bool Includes(int value)
        {
            return SourceFrom <= value && SourceTo >= value;
        }

        /// <summary>
        /// Applies the proportional mapping to the value.
        /// </summary>
        public int Map(int value)
        {
            var srcLength = SourceTo - SourceFrom;
            var destLength = DestTo - DestFrom;
            var coeff = (value - SourceFrom) / (double)srcLength;
            return (int) (DestFrom + (destLength * coeff));
        }
    }
}
