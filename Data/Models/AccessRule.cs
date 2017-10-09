using System;

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
        public virtual  Guid Id { get; set; }

        /// <summary>
        /// User whose access is controlled.
        /// </summary>
        public virtual AppUser User { get; set; }

        /// <summary>
        /// Allowed or restricted page.
        /// </summary>
        public virtual Page Page { get; set; }

        /// <summary>
        /// Access flag.
        /// </summary>
        public virtual bool AllowEditing { get; set; }
    }
}
