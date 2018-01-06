using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Bonsai.Code.Tools
{
    /// <summary>
    /// A date with possibly unknown parts.
    /// </summary>
    public struct FuzzyDate
    {
        #region Constructor

        public FuzzyDate(int? year, int? month, int? day, bool isDecade = false)
        {
            if (day != null && month == null)
                throw new ArgumentException("When day is specified, month must also be specified.");

            // ReSharper disable once ObjectCreationAsStatement
            // invokes internal validation
            new DateTime(year ?? 2000, month ?? 1, day ?? 1);

            Year = year;
            Month = month;
            Day = day;
            IsDecade = isDecade;
        }

        #endregion

        #region Fields

        public readonly int? Year;
        public readonly int? Month;
        public readonly int? Day;

        public readonly bool IsDecade;

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

                if (Day != null)
                {
                    sb.Append(Day);
                    sb.Append(" ");
                }

                if (Month != null)
                {
                    sb.Append(
                        Day != null
                            ? MonthNamesGenitive[Month.Value]
                            : MonthNamesNominative[Month.Value]
                    );
                }

                if (Year != null)
                {
                    if (Month != null)
                        sb.Append(IsDecade ? ", " : " ");

                    sb.Append(ReadableYear);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Readable year component of the date.
        /// </summary>
        public string ReadableYear
        {
            get
            {
                if (Year == null)
                    return null;

                return Year + (IsDecade ? "-е" : "");
            }
        }

        /// <summary>
        /// Returns the day and month if specified (for anniversaries).
        /// </summary>
        public DateTime? Date
        {
            get
            {
                if (Day == null || Month == null)
                    return null;

                return new DateTime(Year ?? DateTime.Now.Year, Month.Value, Day.Value);
            }
        }

        /// <summary>
        /// Returns the number of years since the date.
        /// Returns null for future years.
        /// </summary>
        public string GetAge(DateTime? relative = null)
        {
            var now = relative ?? DateTime.Now;

            if (!(now.Year > Year))
                return null;

            var years = now.Year - Year.Value - 1;

            if (IsDecade)
                return $"{years - 10}..{years + 1} {GetAgeWord(years + 1)}";

            if (now.Month == Month && Day == null)
                return $"{years}..{years + 1} {GetAgeWord(years + 1)}";

            // account for after-birthday
            if (now.Month > Month || (now.Month == Month && now.Day > Day))
                years++;

            return $"{years} {GetAgeWord(years)}";
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

        /// <summary>
        /// Safely parses a FuzzyDate.
        /// </summary>
        public static FuzzyDate? TryParse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return null;

            try
            {
                return Parse(raw);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a fuzzy date from a string.
        /// </summary>
        public static FuzzyDate Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                throw new ArgumentNullException(nameof(raw));

            var match = FormatRegex.Match(raw);
            if(!match.Success)
                throw new ArgumentException("Date string was not in correct format!", nameof(raw));

            var year = ParseInt(match.Groups["year"].Value);
            var month = ParseInt(match.Groups["month"].Value);
            var day = ParseInt(match.Groups["day"].Value);
            var isDecade = false;

            if (year == null)
            {
                // parse 199? as 1990
                year = ParseInt(match.Groups["year"].Value.Replace('?', '0'));
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
        private static int? ParseInt(string str)
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

            if (Year == null)
                sb.Append("????");
            else if (IsDecade)
                sb.Append((Year.Value / 10) + "?");
            else
                sb.Append(Year.Value);

            sb.Append(".");
            sb.Append(Month == null ? "??" : Month.ToString());

            sb.Append(".");
            sb.Append(Day == null ? "??" : Day.ToString());

            return sb.ToString();
        }

        #endregion

        #region JsonConverter

        public class FuzzyDateJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return FuzzyDate.Parse(reader.ReadAsString());
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(FuzzyDate);
            }
        }

        #endregion
    }
}
