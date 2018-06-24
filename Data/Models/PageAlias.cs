using System;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Map between page names and actual pages.
    /// </summary>
    public class PageAlias
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Page key (for URL mapping).
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Related page.
        /// </summary>
        public Page Page { get; set; }
    }
}
