using Bonsai.Code.Services;

namespace Bonsai.Code.Utils.Helpers;

/// <summary>
/// Helper methods for working with numbers.
/// </summary>
public static class NumberHelper
{
    /// <summary>
    /// Returns the string formatted with the correct version of the word for it.
    /// </summary>
    public static string DisplayNumeric(this int number, string forms, bool addNumber = true)
    {
        var form = LocaleProvider.GetNumericWord(number, forms);
        return addNumber ? $"{number} {form}" : form;
    }
}