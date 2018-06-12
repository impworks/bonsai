using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        public UserManagerService(AppDbContext db, UserManager<AppUser> userMgr, IMapper mapper)
        {
            _db = db;
            _userMgr = userMgr;
            _mapper = mapper;
        }

        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userMgr;
        private readonly IMapper _mapper;

        /// <summary>
        /// Returns the list of all registered users.
        /// </summary>
        public async Task<UserListVM> GetUsersListAsync()
        {
            var roles = await _db.Roles
                                 .ToDictionaryAsync(x => x.Id, x => Enum.Parse<UserRole>(x.Name))
                                 .ConfigureAwait(false);

            var userBindings = await _db.UserRoles
                                        .ToDictionaryAsync(x => x.UserId, x => x.RoleId)
                                        .ConfigureAwait(false);

            var users = await _db.Users
                                 .Where(x => x.LockoutEnd != DateTimeOffset.MaxValue)
                                 .ProjectTo<UserTitleVM>()
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
        /// Retrieves the default values for an update operation.
        /// </summary>
        public async Task<UpdateUserVM> RequestUpdateAsync(string id)
        {
            var user = await _db.Users
                                .AsNoTracking()
                                .GetAsync(x => x.Id == id, "Пользователь не найден")
                                .ConfigureAwait(false);

            var vm = _mapper.Map<UpdateUserVM>(user);

            var roles = await _userMgr.GetRolesAsync(user).ConfigureAwait(false);
            if (roles.Count > 0 && Enum.TryParse<UserRole>(roles.First(), out var role))
                vm.Role = role;

            return vm;
        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        public async Task UpdateAsync(UpdateUserVM request, ClaimsPrincipal currUser)
        {
            await ValidateUpdateRequestAsync(request).ConfigureAwait(false);

            var user = await _db.Users
                                .GetAsync(x => x.Id == request.Id, "Пользователь не найден")
                                .ConfigureAwait(false);

            _mapper.Map(request, user);
            user.IsValidated = true;

            if(!IsSelf(request.Id, currUser))
            {
                var allRoles = EnumHelper.GetEnumValues<UserRole>().Select(x => x.ToString());
                await _userMgr.RemoveFromRolesAsync(user, allRoles).ConfigureAwait(false);

                var role = request.Role.ToString();
                await _userMgr.AddToRoleAsync(user, role).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves the information about user removal.
        /// </summary>
        public async Task<RemoveUserVM> RequestRemoveAsync(string id)
        {
            var user = await _db.Users
                                .AsNoTracking()
                                .GetAsync(x => x.Id == id, "Пользователь не найден")
                                .ConfigureAwait(false);

            return _mapper.Map<RemoveUserVM>(user);
        }

        /// <summary>
        /// Removes the user account.
        /// </summary>
        public async Task RemoveAsync(string id, ClaimsPrincipal currUser)
        {
            if(IsSelf(id, currUser))
                throw new OperationException("Нельзя удалить собственную учетную запись");

            var user = await _db.Users
                                .Include(x => x.Changes)
                                .Include(x => x.Page)
                                .GetAsync(x => x.Id == id, "Пользователь не найден")
                                .ConfigureAwait(false);

            if (user.Page == null && user.Changes.Count == 0)
            {
                await _userMgr.DeleteAsync(user).ConfigureAwait(false);
                return;
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }

        /// <summary>
        /// Checks if the specified ID belongs to current user.
        /// </summary>
        public bool IsSelf(string id, ClaimsPrincipal principal)
        {
            return principal != null
                   && _userMgr.GetUserId(principal) == id;
        }

        #region Private helpers

        /// <summary>
        /// Performs additional checks on the request.
        /// </summary>
        private async Task ValidateUpdateRequestAsync(UpdateUserVM request)
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
