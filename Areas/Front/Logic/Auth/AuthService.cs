using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Tools;
using Bonsai.Code.Utils;
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
        public AuthService(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AppDbContext db)
        {
            _signMgr = signInManager;
            _userMgr = userManager;
            _db = db;
        }

        private readonly SignInManager<AppUser> _signMgr;
        private readonly UserManager<AppUser> _userMgr;
        private readonly AppDbContext _db;

        /// <summary>
        /// Attempts to authenticate the user.
        /// </summary>
        public async Task<LoginResultVM> LoginAsync()
        {
            var info = await _signMgr.GetExternalLoginInfoAsync().ConfigureAwait(false);
            if (info == null)
                return new LoginResultVM(LoginStatus.Failed);

            var result = await _signMgr.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true).ConfigureAwait(false);
            if (result.IsLockedOut || result.IsNotAllowed)
                return new LoginResultVM(LoginStatus.LockedOut, info);

            if(!result.Succeeded)
                return new LoginResultVM(LoginStatus.NewUser, info);

            var user = await FindUserAsync(info.LoginProvider, info.ProviderKey).ConfigureAwait(false);
            if (!user.IsValidated)
                return new LoginResultVM(LoginStatus.Unvalidated, info);

            return new LoginResultVM(LoginStatus.Succeeded, info);
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        public async Task LogoutAsync()
        {
            await _signMgr.SignOutAsync().ConfigureAwait(false);
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
            var errors = await ValidateRegisterRequestAsync(vm).ConfigureAwait(false);
            if (errors.Any())
                return new RegisterUserResultVM(errors);

            var id = Guid.NewGuid().ToString();
            var user = new AppUser
            {
                Id = id,
                Email = vm.Email,
                UserName = Regex.Replace(vm.Email, "[^a-z0-9]", ""),
                FirstName = vm.FirstName,
                MiddleName = vm.MiddleName,
                LastName = vm.LastName,
                Birthday = vm.Birthday
            };

            var createResult = await _userMgr.CreateAsync(user).ConfigureAwait(false);
            if (!createResult.Succeeded)
            {
                var msgs = createResult.Errors.Select(x => new KeyValuePair<string, string>("", x.Description)).ToList();
                return new RegisterUserResultVM (msgs);
            }

            var login = new UserLoginInfo(extLogin.LoginProvider, extLogin.ProviderKey, extLogin.LoginProvider);
            await _userMgr.AddLoginAsync(user, login).ConfigureAwait(false);

            await _signMgr.SignInAsync(user, true).ConfigureAwait(false);

            return new RegisterUserResultVM();
        }

        /// <summary>
        /// Returns the information about the current user.
        /// </summary>
        public async Task<UserVM> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;

            var user = await _userMgr.GetUserAsync(principal).ConfigureAwait(false)
                       ?? await FindUserAsync(principal.Identity.AuthenticationType, _userMgr.GetUserId(principal)).ConfigureAwait(false);

            if (user == null)
                return null;

            var isAdmin = await _userMgr.IsInRoleAsync(user, RoleNames.AdminRole).ConfigureAwait(false);

            return new UserVM
            {
                Name = user.FirstName + " " + user.LastName,
                Avatar = GetGravatarUrl(user.Email),
                PageKey = user.Page?.Key,
                IsAdministrator = isAdmin,
                IsValidated = user.IsValidated
            };
        }

        #region Private helpers

        /// <summary>
        /// Finds the corresponding user.
        /// </summary>
        private async Task<AppUser> FindUserAsync(string provider, string key)
        {
            var login = await _db.UserLogins
                                 .FirstOrDefaultAsync(x => x.LoginProvider == provider
                                                           && x.ProviderKey == key)
                                 .ConfigureAwait(false);

            if (login == null)
                return null;

            return await _db.Users
                            .FirstOrDefaultAsync(x => x.Id == login.UserId)
                            .ConfigureAwait(false);
        }

        /// <summary>
        /// Performs additional checks on the registration request.
        /// </summary>
        private async Task<IReadOnlyList<KeyValuePair<string, string>>> ValidateRegisterRequestAsync(RegisterUserVM vm)
        {
            var result = new Dictionary<string, string>();

            if (FuzzyDate.TryParse(vm.Birthday) == null)
                result[nameof(vm.Birthday)] = "Дата рождения указана неверно.";

            var emailExists = await _db.Users.AnyAsync(x => x.Email == vm.Email).ConfigureAwait(false);
            if (emailExists)
                result[nameof(vm.Email)] = "Адрес электронной почты уже зарегистрирован.";

            return result.ToList();
        }

        /// <summary>
        /// Returns the Gravatar URL for a given email.
        /// </summary>
        private string GetGravatarUrl(string email)
        {
            var cleanEmail = (email ?? "").ToLowerInvariant().Trim();
            var md5 = StringHelper.Md5(cleanEmail);
            return "https://www.gravatar.com/avatar/" + md5;
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
