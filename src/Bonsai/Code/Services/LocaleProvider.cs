using System;
using System.Linq;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// Helper service for managing supported locales.
    /// </summary>
    public static class LocaleProvider
    {
        private static readonly LocaleBase[] Locales =
        [
            new LocaleRu(),
            new LocaleEn()
        ];

        private static LocaleBase CurrentLocale;

        /// <summary>
        /// Checks if the specified locale is supported.
        /// </summary>
        public static bool IsSupported(string code) => Locales.Any(x => x.Code == code);

        /// <summary>
        /// Sets the current locale.
        /// </summary>
        public static void SetLocale(string code)
        {
            CurrentLocale = Locales.FirstOrDefault(x => x.Code == code) ?? Locales[0];
        }

        /// <summary>
        /// Returns the string code of the current locale (e.g. ru-RU).
        /// </summary>
        public static string GetLocaleCode()
        {
            if (CurrentLocale == null)
                throw new Exception("Current locale is not set.");

            return CurrentLocale.Code;
        }

        /// <summary>
        /// Returns the appropriate word form for the number (e.g. 1 - object, 2 - objects).
        /// </summary>
        public static string GetNumericWord(int word, string forms)
        {
            if (CurrentLocale == null)
                throw new Exception("Current locale is not set.");

            var formsArray = forms.Split('|');
            try
            {
                return CurrentLocale.GetNumericWord(word, formsArray);
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException($"Invalid pluralization form '{forms}' for locale {CurrentLocale.Code}!");
            }
        }

        #region Locale implementations

        /// <summary>
        /// Base class for locale implementations.
        /// </summary>
        private abstract record LocaleBase(string Code)
        {
            public abstract string GetNumericWord(int number, string[] forms);
        }

        /// <summary>
        /// Russian locale.
        /// </summary>
        private record LocaleRu() : LocaleBase("ru-RU")
        {
            public override string GetNumericWord(int number, string[] forms)
            {
                var ones = number % 10;
                var tens = (number / 10) % 10;

                if (tens != 1)
                {
                    if (ones == 1)
                        return forms[0];

                    if (ones >= 2 && ones <= 4)
                        return forms[1];
                }

                return forms[2];
            }
        }

        /// <summary>
        /// English locale.
        /// </summary>
        private record LocaleEn() : LocaleBase("en-US")
        {
            public override string GetNumericWord(int number, string[] forms)
            {
                return number == 1 ? forms[0] : forms[1];
            }
        }

        #endregion
    }
}
