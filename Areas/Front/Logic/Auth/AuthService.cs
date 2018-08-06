using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
            await ValidateRegisterRequestAsync(vm).ConfigureAwait(false);

            var isFirstUser = (await _db.Users.AnyAsync().ConfigureAwait(false)) == false;

            var user = _mapper.Map<AppUser>(vm);
            user.Id = Guid.NewGuid().ToString();
            user.IsValidated = isFirstUser;

            var createResult = await _userMgr.CreateAsync(user).ConfigureAwait(false);
            if (!createResult.Succeeded)
            {
                var msgs = createResult.Errors.Select(x => new KeyValuePair<string, string>("", x.Description)).ToList();
                throw new ValidationException(msgs);
            }

            var login = new UserLoginInfo(extLogin.LoginProvider, extLogin.ProviderKey, extLogin.LoginProvider);
            await _userMgr.AddLoginAsync(user, login).ConfigureAwait(false);

            await _userMgr.AddToRoleAsync(user, isFirstUser ? nameof(UserRole.Admin) : nameof(UserRole.User)).ConfigureAwait(false);

            await _signMgr.SignInAsync(user, true).ConfigureAwait(false);

            return new RegisterUserResultVM
            {
                IsValidated = user.IsValidated
            };
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

            var roles = await _userMgr.GetRolesAsync(user).ConfigureAwait(false);
            var isAdmin = roles.Contains(nameof(UserRole.Admin)) || roles.Contains(nameof(UserRole.Editor));

            return new UserVM
            {
                Name = user.FirstName + " " + user.LastName,
                Avatar = ViewHelper.GetGravatarUrl(user.Email),
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
        private async Task ValidateRegisterRequestAsync(RegisterUserVM vm)
        {
            var val = new Validator();

            if (FuzzyDate.TryParse(vm.Birthday) == null)
                val.Add(nameof(vm.Birthday), "Дата рождения указана неверно.");

            var emailExists = await _db.Users.AnyAsync(x => x.Email == vm.Email).ConfigureAwait(false);
            if (emailExists)
                val.Add(nameof(vm.Email), "Адрес электронной почты уже зарегистрирован.");

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
