using System;
using System.ComponentModel.DataAnnotations;

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
        /// Original title.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Page key (for URL mapping).
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Key { get; set; }

        /// <summary>
        /// Order of the fact in editor.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Related page.
        /// </summary>
        public Page Page { get; set; }
    }
}
