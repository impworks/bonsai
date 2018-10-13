using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Temporary state of the page's editor.
    /// </summary>
    public class PageDraft
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the edited page (if any).
        /// </summary>
        public Guid? PageId { get; set; }

        /// <summary>
        /// ID of the user.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Editor.
        /// </summary>
        [Required]
        public AppUser User { get; set; }

        /// <summary>
        /// Serialized state of the editor.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// Date of the page's last revision.
        /// </summary>
        public DateTimeOffset LastUpdateDate { get; set; }
    }
}
