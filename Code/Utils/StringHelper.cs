using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bonsai.Code.Utils
{
    /// <summary>
    /// Useful methods for working with strings.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Returns the first non-empty string in the list.
        /// </summary>
        public static string Coalesce(params string[] items)
        {
            return items.FirstOrDefault(x => !string.IsNullOrEmpty(x));
        }

        /// <summary>
        /// Checks if the two strings start with the same sequence.
        /// </summary>
        public static bool StartsWithPart(this string first, string second, int comparisonLength, bool ignoreCase = true)
        {
            if (comparisonLength > first.Length || comparisonLength > second.Length)
                return string.Compare(first, second, ignoreCase) == 0;

            return string.Compare(first, 0, second, 0, comparisonLength, ignoreCase) == 0;
        }

        /// <summary>
        /// Checks if the two strings end with the same sequence.
        /// </summary>
        public static bool EndsWithPart(this string first, string second, int comparisonLength, bool ignoreCase = true)
        {
            if(comparisonLength > first.Length || comparisonLength > second.Length)
                return string.Compare(first, second, ignoreCase) == 0;

            var cmp = string.Compare(
                first,
                first.Length - comparisonLength,
                second,
                second.Length - comparisonLength,
                comparisonLength,
                ignoreCase
            );

            return cmp == 0;
        }

        /// <summary>
        /// Converts the first character of the string to upper case.
        /// </summary>
        public static string Capitalize(this string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// Returns the MD5 hash of a string.
        /// </summary>
        public static string Md5(string str)
        {
            using (var md5 = MD5.Create())
            {
                var input = Encoding.UTF8.GetBytes(str);
                var output = md5.ComputeHash(input);

                var sb = new StringBuilder();
                foreach (var b in output)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
    }
}
