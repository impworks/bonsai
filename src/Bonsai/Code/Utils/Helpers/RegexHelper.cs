using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Bonsai.Code.Utils.Helpers;

/// <summary>
/// Helper utilities for working with regular expressions. 
/// </summary>
public static class RegexHelper
{
    /// <summary>
    /// Fluently replaces a string part using regex.
    /// </summary>
    public static string ReplaceRegex(this string source, [RegexPattern]string pattern, string replacement)
    {
        return Regex.Replace(source, pattern, replacement, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
    }
}