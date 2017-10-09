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
        /// Changes created by this user.
        /// </summary>
        public virtual ICollection<Changeset> Changes { get; set; }

        /// <summary>
        /// List of this user's access rules.
        /// </summary>
        public virtual ICollection<AccessRule> AccessRules { get; set; }
    }
}
