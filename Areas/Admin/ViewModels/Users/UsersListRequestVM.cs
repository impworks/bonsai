using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// Request for filtering users.
    /// </summary>
    public class UsersListRequestVM: ListRequestVM
    {
        /// <summary>
        /// List of selected page types.
        /// </summary>
        public UserRole[] Roles { get; set; }
    }
}
