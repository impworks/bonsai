using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// A rule allowing or restricting access.
    /// </summary>
    public class AccessRule
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User whose access is controlled.
        /// </summary>
        [Required]
        public AppUser User { get; set; }

        /// <summary>
        /// Allowed or restricted page.
        /// </summary>
        [Required]
        public Page Page { get; set; }

        /// <summary>
        /// Access flag.
        /// </summary>
        public bool AllowEditing { get; set; }
    }
}
