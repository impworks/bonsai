using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
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
        /// Attempts to authorize the user.
        /// </summary>
        public async Task<AuthResult> AuthorizeAsync(HttpContext http)
        {
            var authResult = await http.AuthenticateAsync(ExternalCookieAuthType).ConfigureAwait(false);
            var loginData = GetUserLoginData(authResult?.Principal);

            if (loginData == null)
                return AuthResult.Failed;

            var user = await FindUserAsync(loginData).ConfigureAwait(false);
            if (user == null)
            {
                await CreateUserAsync(loginData).ConfigureAwait(false);
                return AuthResult.ValidationPending;
            }

            var isLockedOut = await _userMgr.IsLockedOutAsync(user).ConfigureAwait(false);
            if (isLockedOut)
                return AuthResult.LockedOut;

            if (user.IsValidated == false)
                return AuthResult.Unvalidated;

            await _signMgr.SignInAsync(user, true).ConfigureAwait(false);
            return AuthResult.Succeeded;
        }

        #region Constants

        public const string UserRoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        public const string UserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        public const string UserEmailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        public const string UserNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

        public const string ExternalCookieAuthType = "ExternalCookie";

        #endregion

        #region Private helpers

        /// <summary>
        /// Returns the information about a user.
        /// </summary>
        private UserLoginData GetUserLoginData(ClaimsPrincipal principal)
        {
            var identity = principal?.Identity as ClaimsIdentity;
            var claim = identity?.FindFirst(UserIdClaimType);

            if (claim == null)
                return null;

            return new UserLoginData(identity, claim.Issuer, claim.Value);
        }

        /// <summary>
        /// Finds the corresponding user.
        /// </summary>
        private async Task<AppUser> FindUserAsync(UserLoginData data)
        {
            var login = await _db.UserLogins
                                 .FirstOrDefaultAsync(x => x.LoginProvider == data.LoginProvider
                                                           && x.ProviderKey == data.ProviderKey)
                                 .ConfigureAwait(false);

            if (login == null)
                return null;

            return await _db.Users
                            .FirstOrDefaultAsync(x => x.Id == login.UserId)
                            .ConfigureAwait(false);
        }

        /// <summary>
        /// Adds the user and its claims.
        /// </summary>
        private async Task<AppUser> CreateUserAsync(UserLoginData login)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
