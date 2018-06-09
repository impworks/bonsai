using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.User;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for handling user accounts.
    /// </summary>
    public class UserManagerService
    {
        public UserManagerService(AppDbContext db, UserManager<AppUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userMgr;

        /// <summary>
        /// Returns the list of all registered users.
        /// </summary>
        public async Task<UserListVM> GetUsersListAsync()
        {
            throw new NotImplementedException();
        }
    }
}
