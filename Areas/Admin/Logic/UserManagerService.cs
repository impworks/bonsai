using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.User;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            var roleDescrs = EnumHelper.GetEnumDescriptions<UserRole>()
                                       .ToDictionary(x => x.Value, x => x.Key);

            var roles = await _db.Roles
                                 .ToDictionaryAsync(x => x.Id, x => roleDescrs[x.Name])
                                 .ConfigureAwait(false);

            var userBindings = await _db.UserRoles
                                        .ToDictionaryAsync(x => x.UserId, x => x.RoleId)
                                        .ConfigureAwait(false);

            var users = await _db.Users
                                 .Select(x => new UserTitleVM
                                 {
                                     Id = x.Id,
                                     FullName = x.FirstName + " " + x.LastName
                                 })
                                 .OrderBy(x => x.FullName)
                                 .ToListAsync()
                                 .ConfigureAwait(false);

            foreach (var user in users)
            {
                if (userBindings.TryGetValue(user.Id, out var roleId))
                    user.Role = roles[roleId];
            }

            return new UserListVM
            {
                Users = users
            };
        }

        /// <summary>
        /// Confirms user validation.
        /// </summary>
        public async Task ValidateAsync(ValidateUserVM request)
        {
            await CheckValidateRequestAsync(request).ConfigureAwait(false);

            var user = await _db.Users
                                .GetAsync(x => x.Id == request.Id, "Пользователь не найден")
                                .ConfigureAwait(false);

            if(user.IsValidated)
                throw new OperationException("Пользователь уже проверен");

            user.FirstName = request.FirstName;
            user.MiddleName = request.MiddleName;
            user.LastName = request.LastName;
            user.Birthday = request.Birthday;

            user.IsValidated = true;

            await _db.SaveChangesAsync().ConfigureAwait(false);

            var role = request.Role.ToString();
            await _userMgr.AddToRoleAsync(user, role).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes the user account.
        /// </summary>
        public async Task RemoveAsync(string id)
        {
            var user = await _db.Users
                                .GetAsync(x => x.Id == id, "Пользователь не найден")
                                .ConfigureAwait(false);

            _db.Remove(user);

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        #region Private helpers

        /// <summary>
        /// Performs additional checks on the request.
        /// </summary>
        private async Task CheckValidateRequestAsync(ValidateUserVM request)
        {
            var val = new Validator();

            var emailUsed = await _db.Users
                                     .AnyAsync(x => x.Id != request.Id && x.Email == request.Email)
                                     .ConfigureAwait(false);

            if (emailUsed)
                val.Add(nameof(request.Email), "Адрес почты уже используется другим пользователем");

            val.ThrowIfInvalid();
        }

        #endregion
    }
}
