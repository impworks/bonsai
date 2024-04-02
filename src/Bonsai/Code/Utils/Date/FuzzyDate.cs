﻿using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Bonsai.Code.Services;
using Bonsai.Localization;
using Impworks.Utils.Strings;

namespace Bonsai.Code.Utils.Date
{
    /// <summary>
    /// A date with possibly unknown parts.
    /// </summary>
    public partial struct FuzzyDate: IEquatable<FuzzyDate>, IComparable<FuzzyDate>
    {
        #region Constructor

        public FuzzyDate(DateTime date)
            : this(date.Year, date.Month, date.Day)
        {

        }

        public FuzzyDate(int? year, int? month, int? day, bool isDecade = false)
        {
            if (day == null && month == null && year == null)
                throw new ArgumentNullException(nameof(year), "At least one of the date components must be specified.");

            if (day != null && month == null)
                throw new ArgumentNullException(nameof(month), "When day is specified, month must also be specified.");

            // ReSharper disable once ObjectCreationAsStatement
            // invokes internal validation
            new DateTime(year ?? 2000, month ?? 1, day ?? 1);

            IsDecade = isDecade;
            Year = year;
            Month = month;
            Day = day;

            _stringValue = null;
            _fullReadableDate = null;
            _shortReadableDate = null;
        }

        #endregion

        #region Fields

        /// <summary>
        /// Year component of the date.
        /// </summary>
        public readonly int? Year;

        /// <summary>
        /// Month component of the date.
        /// </summary>
        public readonly int? Month;

        /// <summary>
        /// Day component of the date.
        /// </summary>
        public readonly int? Day;

        /// <summary>
        /// Flag indicating that the year is approximate.
        /// </summary>
        public readonly bool IsDecade;

        /// <summary>
        /// Canonically formatted date.
        /// </summary>
        private string _stringValue;

        /// <summary>
        /// Cached short readable date.
        /// </summary>
        private string _shortReadableDate;

        /// <summary>
        /// Cached readable date.
        /// </summary>
        private string _fullReadableDate;

        #endregion

        #region Properties

        /// <summary>
        /// Short date format (day/month/year).
        /// </summary>
        public string ShortReadableDate => _shortReadableDate ??= GetShortReadableDate();

        /// <summary>
        /// Full readable description of the date.
        /// </summary>
        public string ReadableDate => _fullReadableDate ??= GetFullReadableDate();

        /// <summary>
        /// Readable year component of the date.
        /// </summary>
        public string ReadableYear => Year == null ? null : string.Format(IsDecade ? Texts.FuzzyDate_YearFormat : Texts.FuzzyDate_DecadeFormat, Year);

        /// <summary>
        /// Returns the day and month if specified (for anniversaries).
        /// </summary>
        public DateTime? AnniversaryDate => Day == null || Month == null ? null : new DateTime(Year ?? DateTime.Now.Year, Month.Value, Day.Value);

        /// <summary>
        /// Gets the canonical string representation.
        /// </summary>
        public string CanonicalString => _stringValue ??= GetCanonicalString();

        /// <summary>
        /// Returns true if all components of the fuzzy date are specified precisely.
        /// </summary>
        public bool IsPrecise => Day != null && Month != null && Year != null && !IsDecade;

        /// <summary>
        /// Returns the readable age in years and months.
        /// Returns null for future years.
        /// </summary>
        public string GetAge(FuzzyDate? relative = null)
        {
            var now = relative ?? Now;

            if (!(this <= now) || Year == null || now.Year == null)
                return null;

            var years = now.Year.Value - Year.Value - 1;

            if (IsDecade || now.IsDecade)
            {
                var min = Math.Max(years - 10, 1);
                var max = IsDecade && now.IsDecade ? years + 10 : years + 1;
                return string.Format(Texts.FuzzyDate_Age_FullRangeFormat, min, max, YearsWord(max));
            }

            if (Month == null || now.Month == null || Day == null || now.Day == null)
            {
                var max = years + 1;
                if (years < 0)
                    return Texts.FuzzyDate_Age_LessThanAYear;

                if (now.Month > Month)
                    return FullYears(max);

                if (now.Month < Month)
                    return FullYears(years);

                return string.Format(Texts.FuzzyDate_Age_FullRangeFormat, years, max, YearsWord(max));
            }

            var months = now.Month > Month
                ? now.Month.Value - Month.Value - 1
                : 11 - (Month.Value - now.Month.Value);

            if (now.Day >= Day)
                months++;

            if (months == 12)
            {
                months = 0;
                years++;
            }
            else if (now.Month > Month)
            {
                years++;
            }

            if (years == 0 && months == 0)
                return Texts.FuzzyDate_Age_LessThanAMonth;

            var yearsWord = years == 0 ? null : FullYears(years);
            var monthsWord = months == 0 || years >= 3 ? null : FullMonths(months);

            if (yearsWord != null && monthsWord != null)
                return string.Format(Texts.FuzzyDate_Age_FullPreciseFormat, yearsWord, monthsWord);

            return yearsWord ?? monthsWord;

            string YearsWord(int count) => LocaleProvider.GetNumericWord(count, Texts.FuzzyDate_Years);
            string MonthsWord(int count) => LocaleProvider.GetNumericWord(count, Texts.FuzzyDate_Months);
            string FullYears(int count) => string.Format(Texts.FuzzyDate_Age_YearsFormat, count, YearsWord(count));
            string FullMonths(int count) => string.Format(Texts.FuzzyDate_Age_MonthsFormat, count, MonthsWord(count));
        }

        #endregion

        #region Parse methods

        /// <summary>
        /// Returns the FuzzyDate representing today.
        /// </summary>
        public static FuzzyDate Now => new FuzzyDate(DateTime.Now);

        /// <summary>
        /// Formatting string.
        /// </summary>
        private static readonly Regex FormatRegex = new Regex(
            @"^(?<year>[0-9]{3}[0-9?]|\?{4})(\.(?<month>[0-9]{2}|\?\?)(\.(?<day>[0-9]{2}|\?\?))?)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
        );

        /// <summary>
        /// Safely parses a fuzzy date, returning null on error.
        /// </summary>
        public static FuzzyDate? TryParse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return null;

            return raw.TryParse<FuzzyDate?>(x => Parse(x));
        }

        /// <summary>
        /// Parses a fuzzy date from a string.
        /// </summary>
        public static FuzzyDate Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                throw new ArgumentNullException(nameof(raw));

            var match = FormatRegex.Match(raw);
            if (!match.Success)
                throw new ArgumentException("Date string was not in correct format!", nameof(raw));

            var year = match.Groups["year"].Value.TryParse<int?>();
            var month = match.Groups["month"].Value.TryParse<int?>();
            var day = match.Groups["day"].Value.TryParse<int?>();
            var isDecade = false;

            if (year == null)
            {
                // parse 199? as 1990
                year = match.Groups["year"].Value.Substring(0, 3).TryParse<int?>() * 10;
                isDecade = year != null;
            }

            return new FuzzyDate(year, month, day, isDecade);
        }

        #endregion

        #region Formatting helpers

        /// <summary>
        /// Renders the short readable date.
        /// </summary>
        private string GetShortReadableDate()
        {
            var sb = new StringBuilder();

            if (Day != null)
            {
                sb.Append(Day?.ToString("D2"));
                sb.Append("/");
            }

            if (Month != null)
            {
                sb.Append(Month?.ToString("D2") ?? "??");
                sb.Append("/");
            }

            if (Year == null)
                sb.Append("????");
            else if (IsDecade)
                sb.Append((Year.Value / 10) + "?");
            else
                sb.Append(Year.Value);

            return sb.ToString();
        }

        /// <summary>
        /// Renders the full readable date.
        /// </summary>
        private string GetFullReadableDate()
        {
            if (Month == null && Day == null)
                return ReadableYear;

            var monthPart = Day == null
                ? MonthNames[Month!.Value]
                : string.Format(MonthPreciseNames[Month!.Value], Day.Value);

            return string.Format(IsDecade ? Texts.FuzzyDate_FullDecadeFormat : Texts.FuzzyDate_FullYearFormat, monthPart);
        }

        /// <summary>
        /// Creates the canonical formatting of the date.
        /// </summary>
        private string GetCanonicalString()
        {
            var sb = new StringBuilder();

            if (Year == null)
                sb.Append("????");
            else if (IsDecade)
                sb.Append(Year.Value / 10 + "?");
            else
                sb.Append(Year.Value);

            sb.Append(".");
            sb.Append(Month?.ToString("D2") ?? "??");

            sb.Append(".");
            sb.Append(Day?.ToString("D2") ?? "??");

            return sb.ToString();
        }

        private static string[] MonthNames =
        [
            "",
            Texts.FuzzyDate_January,
            Texts.FuzzyDate_February,
            Texts.FuzzyDate_March,
            Texts.FuzzyDate_April,
            Texts.FuzzyDate_May,
            Texts.FuzzyDate_June,
            Texts.FuzzyDate_July,
            Texts.FuzzyDate_August,
            Texts.FuzzyDate_September,
            Texts.FuzzyDate_October,
            Texts.FuzzyDate_November,
            Texts.FuzzyDate_December
        ];

        private static string[] MonthPreciseNames =
        [
            "",
            Texts.FuzzyDate_JanuaryPrecise,
            Texts.FuzzyDate_FebruaryPrecise,
            Texts.FuzzyDate_MarchPrecise,
            Texts.FuzzyDate_AprilPrecise,
            Texts.FuzzyDate_MayPrecise,
            Texts.FuzzyDate_JunePrecise,
            Texts.FuzzyDate_JulyPrecise,
            Texts.FuzzyDate_AugustPrecise,
            Texts.FuzzyDate_SeptemberPrecise,
            Texts.FuzzyDate_OctoberPrecise,
            Texts.FuzzyDate_NovemberPrecise,
            Texts.FuzzyDate_DecemberPrecise
        ];

        #endregion

        #region IEquatable implementation

        public bool Equals(FuzzyDate other) => CanonicalString == other.CanonicalString;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FuzzyDate date && Equals(date);
        }

        public override int GetHashCode() => CanonicalString.GetHashCode();

        public static bool operator ==(FuzzyDate left, FuzzyDate right) => left.Equals(right);
        public static bool operator !=(FuzzyDate left, FuzzyDate right) => !left.Equals(right);

        #endregion

        #region IComparable implementation

        /// <summary>
        /// Compares two dates for sorting.
        /// </summary>
        public int CompareTo(FuzzyDate other)
        {
            return string.Compare(CanonicalString, other.CanonicalString, CultureInfo.InvariantCulture, CompareOptions.None);
        }

        public static bool operator >=(FuzzyDate first, FuzzyDate second)
        {
            return first.CompareTo(second) >= 0;
        }

        public static bool operator <=(FuzzyDate first, FuzzyDate second)
        {
            return first.CompareTo(second) <= 0;
        }

        public static bool operator >(FuzzyDate first, FuzzyDate second)
        {
            return first.CompareTo(second) > 0;
        }

        public static bool operator <(FuzzyDate first, FuzzyDate second)
        {
            return first.CompareTo(second) < 0;
        }

        public static bool operator >=(FuzzyDate? first, FuzzyDate? second)
        {
            return first != null
                   && second != null
                   && first.Value.CompareTo(second.Value) >= 0;
        }

        public static bool operator <=(FuzzyDate? first, FuzzyDate? second)
        {
            return first != null
                   && second != null
                   && first.Value.CompareTo(second.Value) <= 0;
        }

        public static bool operator >(FuzzyDate? first, FuzzyDate? second)
        {
            return first != null
                   && second != null
                   && first.Value.CompareTo(second.Value) > 0;
        }

        public static bool operator <(FuzzyDate? first, FuzzyDate? second)
        {
            return first != null
                   && second != null
                   && first.Value.CompareTo(second.Value) < 0;
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns the canonically formatted date.
        /// </summary>
        public override string ToString() => CanonicalString;

        #endregion
    }
}
