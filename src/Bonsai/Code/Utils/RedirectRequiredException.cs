using System;

namespace Bonsai.Code.Utils
{
    /// <summary>
    /// An exception that occurs when the page has been accessed via an old alias.
    /// </summary>
    public class RedirectRequiredException: Exception
    {
        public RedirectRequiredException(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Actual key for the page.
        /// </summary>
        public string Key { get; }
    }
}
