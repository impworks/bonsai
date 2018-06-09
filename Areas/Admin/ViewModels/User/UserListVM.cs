using System.Collections.Generic;

namespace Bonsai.Areas.Admin.ViewModels.User
{
    /// <summary>
    /// List of users.
    /// </summary>
    public class UserListVM
    {
        /// <summary>
        /// List of registered users.
        /// </summary>
        public IReadOnlyList<UserTitleVM> Users { get; set; }
    }
}
