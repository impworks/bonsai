using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// User account.
    /// </summary>
    public class AppUser: IdentityUser
    {
        /// <summary>
        /// User's own page.
        /// </summary>
        public Page Page { get; set; }

        /// <summary>
        /// Changes created by this user.
        /// </summary>
        public ICollection<Changeset> Changes { get; set; }

        /// <summary>
        /// List of this user's access rules.
        /// </summary>
        public ICollection<AccessRule> AccessRules { get; set; }
    }
}
