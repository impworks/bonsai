namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper methods for working with numbers.
    /// </summary>
    public static class NumberHelper
    {
        /// <summary>
        /// Returns the string formatted with the correct version of the word for it.
        /// </summary>
        public static string DisplayNumeric(this int number, string one, string two, string five, bool addNumber = true)
        {
            return addNumber ?
                number + " " + GetWordForm()
                : GetWordForm();

            string GetWordForm()
            {
                var d1 = number % 10;
                var d2 = (number / 10) % 10;

                if (d2 != 1)
                {
                    if (d1 == 1)
                        return one;
                    if (d1 > 1 && d1 < 5)
                        return two;
                }

                return five;
            }
        }
    }
}
