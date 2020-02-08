using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// Additional data for the user editor page.
    /// </summary>
    public class UserEditorDataVM
    {
        /// <summary>
        /// Flag indicating that an admin is editing their own profile.
        /// </summary>
        public bool IsSelf { get; set; }

        /// <summary>
        /// Flag indicating that a personal page does not exist yet for this profile.
        /// </summary>
        public bool CanCreatePersonalPage { get; set; }

        /// <summary>
        /// Available roles for the user.
        /// </summary>
        public IReadOnlyList<SelectListItem> UserRoleItems { get; set; }

        /// <summary>
        /// Selected page (if any).
        /// </summary>
        public IReadOnlyList<SelectListItem> PageItems { get; set; }
    }
}
