using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Bonsai.Code.Tools
{
    /// <summary>
    /// A date with possibly unknown parts.
    /// </summary>
    public struct FuzzyDate
    {
        #region Fields

        private readonly bool _isDecade;
        private readonly int? _year;
        private readonly int? _month;
        private readonly int? _day;

        #endregion

        #region Properties

        /// <summary>
        /// Full readable description of the date.
        /// </summary>
        public string ReadableDate
        {
            get
            {
                var sb = new StringBuilder();

                if (_day != null)
                {
                    sb.Append(_day);
                    sb.Append(" ");
                }

                if (_month != null)
                {
                    sb.Append(
                        _day != null
                            ? MonthNamesGenitive[_month.Value]
                            : MonthNamesNominative[_month.Value]
                    );
                }

                if (_year != null)
                {
                    if (_month != null)
                        sb.Append(_isDecade ? ", " : " ");

                    sb.Append(Year);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Year component of the date.
        /// </summary>
        public string Year
        {
            get
            {
                if (_year == null)
                    return null;

                return _year + (_isDecade ? "-е" : "");
            }
        }

        /// <summary>
        /// Returns the day and month if specified (for anniversaries).
        /// </summary>
        public DateTime? Date
        {
            get
            {
                if (_day == null || _month == null)
                    return null;

                return new DateTime(_year ?? DateTime.Now.Year, _month.Value, _day.Value);
            }
        }

        /// <summary>
        /// Returns the number of years since the date.
        /// Returns null for future years.
        /// </summary>
        public string Age
        {
            get
            {
                var now = DateTime.Now;
                if (!(now.Year > _year))
                    return null;

                var years = now.Year - _year.Value - 1;

                if (_isDecade)
                    return $"{years - 10}..{years + 1} {GetAgeWord(years + 1)}";

                if (now.Month == _month && _day == null)
                    return $"{years}..{years + 1} {GetAgeWord(years + 1)}";

                // account for after-birthday
                if (now.Month > _month || (now.Month == _month && now.Day > _day))
                    years++;

                return $"{years} {GetAgeWord(years)}";
            }
        }

        #endregion

        #region Parse methods

        /// <summary>
        /// Formatting string.
        /// </summary>
        private static readonly Regex FormatRegex = new Regex(
            @"(?<year>[0-9]{3}[0-9?]|\?{4})\.(?<month>[0-9]{2}|\?\?)\.(?<day>[0-9]{2}|\?\?)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
        );

        private FuzzyDate(int? year, int? month, int? day, bool isDecade = false)
        {
            if (day != null && month == null)
                throw new ArgumentException("When day is specified, month must also be specified.");

            // ReSharper disable once ObjectCreationAsStatement
            // invokes internal validation
            new DateTime(year ?? 2000, month ?? 1, day ?? 1);

            _year = year;
            _month = month;
            _day = day;
            _isDecade = isDecade;
        }

        /// <summary>
        /// Parses a fuzzy date from a string.
        /// </summary>
        public static FuzzyDate FromString(string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            var match = FormatRegex.Match(str);
            if(!match.Success)
                throw new ArgumentException("Date string was not in correct format!", nameof(str));

            var year = Parse(match.Groups["year"].Value);
            var month = Parse(match.Groups["month"].Value);
            var day = Parse(match.Groups["day"].Value);
            var isDecade = false;

            if (year == null)
            {
                // parse 199? as 1990
                year = Parse(match.Groups["year"].Value.Replace('?', '0'));
                isDecade = year != null;
            }

            return new FuzzyDate(year, month, day, isDecade);
        }

        #endregion

        #region Formatting helpers

        /// <summary>
        /// Month names in nominative case (for month + year format).
        /// </summary>
        private static string[] MonthNamesNominative =
        {
            "",
            "январь",
            "февраль",
            "март",
            "апрель",
            "май",
            "июнь",
            "июль",
            "август",
            "сентябрь",
            "октябрь",
            "ноябрь",
            "декабрь"
        };

        /// <summary>
        /// Month names in genitive case (for day + month format).
        /// </summary>
        private static string[] MonthNamesGenitive =
        {
            "",
            "января",
            "февраля",
            "марта",
            "апреля",
            "мая",
            "июня",
            "июля",
            "августа",
            "сентября",
            "октября",
            "ноября",
            "декабря"
        };

        /// <summary>
        /// Attempts to parse an integer value from a string.
        /// </summary>
        private static int? Parse(string str)
        {
            if (int.TryParse(str, out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Returns the word in correct declension for the number of years.
        /// </summary>
        private static string GetAgeWord(int age)
        {
            var ones = age % 10;
            var tens = (age / 10) % 10;

            if (tens != 1)
            {
                if (ones == 1)
                    return "год";

                if (ones >= 2 && ones <= 4)
                    return "года";
            }

            return "лет";
        }

        #endregion

        #region ToString

        /// <summary>
        /// Formats the date back to original string.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (_year == null)
                sb.Append("????");
            else if (_isDecade)
                sb.Append((_year.Value / 10) + "?");
            else
                sb.Append(_year.Value);

            sb.Append(".");
            sb.Append(_month == null ? "??" : _month.ToString());

            sb.Append(".");
            sb.Append(_day == null ? "??" : _day.ToString());

            return sb.ToString();
        }

        #endregion
    }
}
