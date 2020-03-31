using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic.Auth
{
    /// <summary>
    /// Wrapper service for authorization.
    /// </summary>
    public class AuthService
    {
        public AuthService(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AppDbContext db, IMapper mapper)
        {
            _signMgr = signInManager;
            _userMgr = userManager;
            _db = db;
            _mapper = mapper;
        }

        private readonly SignInManager<AppUser> _signMgr;
        private readonly UserManager<AppUser> _userMgr;
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        /// <summary>
        /// Attempts to authenticate the user via OAuth.
        /// </summary>
        public async Task<LoginResultVM> ExternalLoginAsync()
        {
            var info = await _signMgr.GetExternalLoginInfoAsync();
            if (info == null)
                return new LoginResultVM(LoginStatus.Failed);

            var result = await _signMgr.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            if (result.IsLockedOut || result.IsNotAllowed)
                return new LoginResultVM(LoginStatus.LockedOut, info);

            if(!result.Succeeded)
                return new LoginResultVM(LoginStatus.NewUser, info);

            var user = await FindUserAsync(info.LoginProvider, info.ProviderKey);
            if (!user.IsValidated)
                return new LoginResultVM(LoginStatus.Unvalidated, info);

            return new LoginResultVM(LoginStatus.Succeeded, info);
        }

        /// <summary>
        /// Attempts to authorize the user via local auth.
        /// </summary>
        public async Task<LoginResultVM> LocalLoginAsync(LocalLoginVM vm)
        {
            var email = vm.Login?.ToUpper();
            var user = await _db.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == email);
            if (user != null)
            {
                var info = await _signMgr.PasswordSignInAsync(user, vm.Password, isPersistent: true, lockoutOnFailure: true);
                if (info.Succeeded)
                    return new LoginResultVM(LoginStatus.Succeeded);

                if (info.IsLockedOut)
                    return new LoginResultVM(LoginStatus.LockedOut);
            }

            return new LoginResultVM(LoginStatus.Failed);
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        public async Task LogoutAsync()
        {
            await _signMgr.SignOutAsync();
        }

        /// <summary>
        /// Retrieves default values for registration form from the claims provided by external login provider.
        /// </summary>
        public RegisterUserVM GetRegistrationData(ClaimsPrincipal cp)
        {
            string GetClaim(string type) => cp.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/" + type)?.Value;

            return new RegisterUserVM
            {
                FirstName = GetClaim("givenname"),
                LastName = GetClaim("surname"),
                Email = GetClaim("emailaddress"),
                Birthday = FormatDate(GetClaim("dateofbirth"))
            };
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        public async Task<RegisterUserResultVM> RegisterAsync(RegisterUserVM vm, ExternalLoginData extLogin)
        {
            await ValidateRegisterRequestAsync(vm, usePasswordAuth: extLogin == null);

            var isFirstUser = await IsFirstUserAsync();

            var user = _mapper.Map<AppUser>(vm);
            user.Id = Guid.NewGuid().ToString();
            user.IsValidated = isFirstUser;
            user.AuthType = extLogin == null ? AuthType.Password : AuthType.ExternalProvider;

            var createResult = await _userMgr.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var msgs = createResult.Errors.Select(x => new KeyValuePair<string, string>("", x.Description)).ToList();
                throw new ValidationException(msgs);
            }

            if (extLogin != null)
                await _userMgr.AddLoginAsync(user, new UserLoginInfo(extLogin.LoginProvider, extLogin.ProviderKey, extLogin.LoginProvider));
            else
                await _userMgr.AddPasswordAsync(user, vm.Password);

            await _userMgr.AddToRoleAsync(user, isFirstUser ? nameof(UserRole.Admin) : nameof(UserRole.Unvalidated));

            await _signMgr.SignInAsync(user, true);

            return new RegisterUserResultVM
            {
                IsValidated = user.IsValidated,
                User = user,
                Principal = await _signMgr.CreateUserPrincipalAsync(user)
            };
        }

        /// <summary>
        /// Returns the information about the current user.
        /// </summary>
        public async Task<UserVM> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;

            var id = _userMgr.GetUserId(principal);
            var user = await _db.Users
                                .AsNoTracking()
                                .Include(x => x.Page)
                                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return null;

            var roles = await _userMgr.GetRolesAsync(user);
            var isAdmin = roles.Contains(nameof(UserRole.Admin)) || roles.Contains(nameof(UserRole.Editor));

            return new UserVM
            {
                Name = user.FirstName + " " + user.LastName,
                Email = user.Email,
                PageKey = user.Page?.Key,
                IsAdministrator = isAdmin,
                IsValidated = user.IsValidated
            };
        }

        /// <summary>
        /// Checks if there are users existing in the database.
        /// </summary>
        public async Task<bool> IsFirstUserAsync()
        {
            return await _db.Users.AnyAsync() == false;
        }

        #region Private helpers

        /// <summary>
        /// Finds the corresponding user.
        /// </summary>
        private async Task<AppUser> FindUserAsync(string provider, string key)
        {
            var login = await _db.UserLogins
                                 .FirstOrDefaultAsync(x => x.LoginProvider == provider
                                                           && x.ProviderKey == key);

            if (login == null)
                return null;

            return await _db.Users
                            .FirstOrDefaultAsync(x => x.Id == login.UserId);
        }

        /// <summary>
        /// Performs additional checks on the registration request.
        /// </summary>
        private async Task ValidateRegisterRequestAsync(RegisterUserVM vm, bool usePasswordAuth)
        {
            var val = new Validator();

            if (FuzzyDate.TryParse(vm.Birthday) == null)
                val.Add(nameof(vm.Birthday), "Дата рождения указана неверно.");

            var emailExists = await _db.Users.AnyAsync(x => x.Email == vm.Email);
            if (emailExists)
                val.Add(nameof(vm.Email), "Адрес электронной почты уже зарегистрирован.");

            if (usePasswordAuth)
            {
                if (vm.Password == null || vm.Password.Length < 6)
                    val.Add(nameof(vm.Password), "Пароль должен содержать как минимум 6 символов.");

                if (vm.Password != vm.PasswordCopy)
                    val.Add(nameof(vm.PasswordCopy), "Пароли не совпадают.");
            }

            val.ThrowIfInvalid();
        }

        /// <summary>
        /// Formats the date according to local format.
        /// </summary>
        private string FormatDate(string isoDate)
        {
            if (DateTime.TryParse(isoDate, out var date))
                return new FuzzyDate(date).ToString();

            return null;
        }

        #endregion
    }
}
