using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Users;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for handling user accounts.
    /// </summary>
    public class UsersManagerService
    {
        public UsersManagerService(AppDbContext db, UserManager<AppUser> userMgr, IMapper mapper, BonsaiConfigService config)
        {
            _db = db;
            _userMgr = userMgr;
            _mapper = mapper;
            _demoCfg = config.GetStaticConfig().DemoMode;
        }

        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userMgr;
        private readonly IMapper _mapper;
        private readonly DemoModeConfig _demoCfg;

        /// <summary>
        /// Returns the list of all registered users.
        /// </summary>
        public async Task<UsersListVM> GetUsersAsync(UsersListRequestVM request)
        {
            const int PageSize = 20;

            request = NormalizeListRequest(request);

            var query = await LoadUsersAsync();

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var nq = request.SearchQuery.ToLower();
                query = query.Where(x => x.FullName.ToLower().Contains(nq)
                                         || x.Email.ToLower().Contains(nq));
            }

            if (request.Roles?.Length > 0)
                query = query.Where(x => request.Roles.Contains(x.Role));

            var totalCount = query.Count();
            var items = query.OrderBy(request.OrderBy, request.OrderDescending.Value)
                             .Skip(PageSize * request.Page)
                             .Take(PageSize)
                             .ToList();

            return new UsersListVM
            {
                Items = items,
                PageCount = (int) Math.Ceiling((double) totalCount / PageSize),
                Request = request
            };
        }

        /// <summary>
        /// Finds the user by ID.
        /// </summary>
        public async Task<UserTitleVM> GetAsync(string id)
        {
            var user = await _db.Users
                                .AsNoTracking()
                                .GetAsync(x => x.Id == id, "Пользователь не найден");

            return _mapper.Map<UserTitleVM>(user);
        }

        /// <summary>
        /// Retrieves the default values for an update operation.
        /// </summary>
        public async Task<UserEditorVM> RequestUpdateAsync(string id)
        {
            var user = await _db.Users
                                .AsNoTracking()
                                .GetAsync(x => x.Id == id, "Пользователь не найден");

            ValidateDemoModeRestrictions(user);

            var vm = _mapper.Map<UserEditorVM>(user);

            var roles = await _userMgr.GetRolesAsync(user);
            if (roles.Count > 0 && Enum.TryParse<UserRole>(roles.First(), out var role))
                vm.Role = role;

            return vm;
        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        public async Task<AppUser> UpdateAsync(UserEditorVM request, ClaimsPrincipal currUser)
        {
            await ValidateUpdateRequestAsync(request);

            var user = await _db.Users
                                .GetAsync(x => x.Id == request.Id, "Пользователь не найден");

            ValidateDemoModeRestrictions(user);

            _mapper.Map(request, user);
            user.IsValidated = true;

            if(!IsSelf(request.Id, currUser))
            {
                var allRoles = await _userMgr.GetRolesAsync(user);
                await _userMgr.RemoveFromRolesAsync(user, allRoles);

                var role = request.Role.ToString();
                await _userMgr.AddToRoleAsync(user, role);
            }

            if(request.IsLocked && user.LockoutEnd == null)
                user.LockoutEnd = DateTimeOffset.MaxValue;
            else if (!request.IsLocked && user.LockoutEnd != null)
                user.LockoutEnd = null;

            return user;
        }

        /// <summary>
        /// Retrieves the information about user removal.
        /// </summary>
        public async Task<RemoveUserVM> RequestRemoveAsync(string id, ClaimsPrincipal principal)
        {
            var user = await _db.Users
                                .AsNoTracking()
                                .Include(x => x.Changes)
                                .GetAsync(x => x.Id == id, "Пользователь не найден");

            ValidateDemoModeRestrictions(user);

            return new RemoveUserVM
            {
                Id = id,
                FullName = user.FirstName + " " + user.LastName,
                IsSelf = id == _userMgr.GetUserId(principal),
                IsFullyDeletable = !user.Changes.Any()
            };
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
                                .GetAsync(x => x.Id == id, "Пользователь не найден");

            ValidateDemoModeRestrictions(user);

            if (user.Changes.Any())
                throw new OperationException("Нельзя удалить эту учетную запись");

            var result = await _userMgr.DeleteAsync(user);
            if(!result.Succeeded)
                throw new OperationException("Не удалось удалить учетную запись!");
        }

        /// <summary>
        /// Checks if the specified ID belongs to current user.
        /// </summary>
        public bool IsSelf(string id, ClaimsPrincipal principal)
        {
            return principal != null
                   && _userMgr.GetUserId(principal) == id;
        }

        /// <summary>
        /// Checks if the personal page can be created for this user.
        /// </summary>
        public async Task<bool> CanCreatePersonalPageAsync(UserEditorVM vm)
        {
            return await _db.Users
                            .Where(x => x.Id == vm.Id && x.Page == null)
                            .AnyAsync();
        }

        /// <summary>
        /// Resets the user's password.
        /// </summary>
        public async Task ResetPasswordAsync(UserPasswordEditorVM vm)
        {
            ValidatePasswordForm(vm);

            var user = await _db.Users.GetAsync(x => x.Id == vm.Id, "Пользователь не найден");

            ValidateDemoModeRestrictions(user);

            var token = await _userMgr.GeneratePasswordResetTokenAsync(user);
            var result = await _userMgr.ResetPasswordAsync(user, token, vm.Password);

            if(!result.Succeeded)
                throw new OperationException("Не удалось сменить пароль, попробуйте еще раз.");

            await _userMgr.SetLockoutEndDateAsync(user, null);
        }

        /// <summary>
        /// Creates a new user with login-password auth.
        /// </summary>
        public async Task<AppUser> CreateAsync(UserCreatorVM vm)
        {
            ValidatePasswordForm(vm);
            await ValidateRegisterRequestAsync(vm);

            var user = _mapper.Map<AppUser>(vm);
            user.Id = Guid.NewGuid().ToString();
            user.AuthType = AuthType.Password;
            user.IsValidated = true;

            var createResult = await _userMgr.CreateAsync(user, vm.Password);
            if (!createResult.Succeeded)
            {
                var msgs = createResult.Errors.Select(x => new KeyValuePair<string, string>("", x.Description)).ToList();
                throw new ValidationException(msgs);
            }

            await _userMgr.AddToRoleAsync(user, vm.Role.ToString());

            return user;
        }

        #region Private helpers

        /// <summary>
        /// Returns the request with default/valid values.
        /// </summary>
        private UsersListRequestVM NormalizeListRequest(UsersListRequestVM vm)
        {
            if (vm == null)
                vm = new UsersListRequestVM();

            var orderableFields = new[] {nameof(UserTitleVM.FullName), nameof(UserTitleVM.Email)};
            if (!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if (vm.Page < 0)
                vm.Page = 0;

            if (vm.OrderDescending == null)
                vm.OrderDescending = false;

            return vm;
        }

        /// <summary>
        /// Loads users from the database.
        /// </summary>
        private async Task<IQueryable<UserTitleVM>> LoadUsersAsync()
        {
            var roles = await _db.Roles
                                 .ToDictionaryAsync(x => x.Id, x => Enum.Parse<UserRole>(x.Name));

            var userBindings = await _db.UserRoles
                                        .ToDictionaryAsync(x => x.UserId, x => x.RoleId);

            var users = await _db.Users
                                 .ProjectTo<UserTitleVM>(_mapper.ConfigurationProvider)
                                 .ToListAsync();

            foreach (var user in users)
                user.Role = roles[userBindings[user.Id]];

            return users.AsQueryable();
        }

        /// <summary>
        /// Performs additional checks on the request.
        /// </summary>
        private async Task ValidateUpdateRequestAsync(UserEditorVM request)
        {
            var val = new Validator();

            var emailUsed = await _db.Users
                                     .AnyAsync(x => x.Id != request.Id && x.Email == request.Email);

            if (emailUsed)
                val.Add(nameof(request.Email), "Адрес почты уже используется другим пользователем");

            if (request.PersonalPageId != null)
            {
                var exists = await _db.Pages
                                      .AnyAsync(x => x.Id == request.PersonalPageId);
                if (!exists)
                    val.Add(nameof(request.PersonalPageId), "Страница не существует");
            }

            val.ThrowIfInvalid();
        }

        /// <summary>
        /// Ensures that the password form has been filled correctly.
        /// </summary>
        private void ValidatePasswordForm(IPasswordForm form)
        {
            var val = new Validator();

            if (form.Password == null || form.Password.Length < 6)
                val.Add(nameof(form.Password), "Пароль должен содержать как минимум 6 символов.");

            if (form.Password != form.PasswordCopy)
                val.Add(nameof(form.PasswordCopy), "Пароли не совпадают.");

            val.ThrowIfInvalid();
        }

        /// <summary>
        /// Performs additional checks on the registration request.
        /// </summary>
        private async Task ValidateRegisterRequestAsync(UserCreatorVM vm)
        {
            var val = new Validator();

            if (FuzzyDate.TryParse(vm.Birthday) == null)
                val.Add(nameof(vm.Birthday), "Дата рождения указана неверно.");

            var emailExists = await _db.Users.AnyAsync(x => x.Email == vm.Email);
            if (emailExists)
                val.Add(nameof(vm.Email), "Адрес электронной почты уже зарегистрирован.");

            val.ThrowIfInvalid();
        }

        /// <summary>
        /// Checks if the user can be modified depending on current demo mode.
        /// </summary>
        private void ValidateDemoModeRestrictions(AppUser user)
        {
            if (!_demoCfg.Enabled)
                return;

            if(user.Email == "admin@example.com" && _demoCfg.CreateDefaultAdmin)
                throw new OperationException("Запрещено в демо-режиме");
        }

        #endregion
    }
}
