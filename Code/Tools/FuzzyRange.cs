using System;
using System.Text;

namespace Bonsai.Code.Tools
{
    /// <summary>
    /// A range between two fuzzy dates.
    /// </summary>
    public struct FuzzyRange
    {
        #region Constructor

        public FuzzyRange(FuzzyDate? from, FuzzyDate? to)
        {
            RangeStart = from;
            RangeEnd = to;
        }

        #endregion

        #region Fields

        /// <summary>
        /// Start of the range (if specified).
        /// </summary>
        public readonly FuzzyDate? RangeStart;

        /// <summary>
        /// End of the range (if specified).
        /// </summary>
        public readonly FuzzyDate? RangeEnd;

        #endregion

        #region Properties

        /// <summary>
        /// Flag indicating that the range contains no meaningful information.
        /// </summary>
        public bool IsEmpty => RangeStart == null && RangeEnd == null;

        /// <summary>
        /// Returns the short readable range description.
        /// </summary>
        public string ShortReadableRange
        {
            get
            {
                var yearStart = RangeStart?.Year;
                var yearEnd = RangeEnd?.Year;
                var decadeStart = RangeStart?.IsDecade ?? false;
                var decadeEnd = RangeEnd?.IsDecade ?? false;

                if (yearStart == null && yearEnd == null)
                    return null;

                if (yearStart == yearEnd)
                    return "в " + yearStart.Value + (decadeStart ? "-х" : "");

                var sb = new StringBuilder();
                if (yearStart != null)
                {
                    sb.Append("c ");
                    sb.Append(yearStart.Value);
                    if (decadeStart)
                        sb.Append("-х");
                }

                if (yearEnd != null)
                {
                    if (yearStart != null)
                        sb.Append(" ");
                     
                    sb.Append("по ");
                    sb.Append(yearEnd.Value);
                    if (decadeEnd)
                        sb.Append("-ые");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Returns the detailed readable range.
        /// </summary>
        public string ReadableRange
        {
            get
            {
                if (RangeStart == null && RangeEnd == null)
                    return null;

                if (RangeEnd == null)
                    return "с " + RangeStart;

                if (RangeStart == null)
                    return "по " + RangeEnd;

                return RangeStart + " — " + RangeEnd;
            }
        }

        #endregion

        #region Parsing

        /// <summary>
        /// Parses the range from a serialized representation.
        /// </summary>
        public static FuzzyRange Parse(string raw)
        {
            if(string.IsNullOrEmpty(raw))
                return new FuzzyRange(null, null);

            var parts = raw.Split("-");
            if(parts.Length != 2)
                throw new ArgumentException("Incorrect range format.");

            var from = string.IsNullOrEmpty(parts[0]) ? FuzzyDate.Parse(parts[0]) : (FuzzyDate?) null;
            var to = string.IsNullOrEmpty(parts[1]) ? FuzzyDate.Parse(parts[1]) : (FuzzyDate?)null;

            return new FuzzyRange(from, to);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"{RangeStart}-{RangeEnd}";
        }

        #endregion
    }
}
