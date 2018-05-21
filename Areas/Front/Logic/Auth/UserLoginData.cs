using System.Security.Claims;

namespace Bonsai.Areas.Front.Logic.Auth
{
    /// <summary>
    /// Information about a user's authorization.
    /// </summary>
    public class UserLoginData
    {
        public UserLoginData(ClaimsIdentity identity, string loginProvider, string providerKey)
        {
            Identity = identity;
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
        }

        /// <summary>
        /// Identity bound to current user.
        /// </summary>
        public ClaimsIdentity Identity { get; }

        /// <summary>
        /// External provider: Facebook, Google, etc.
        /// </summary>
        public string LoginProvider { get; }

        /// <summary>
        /// User's personal key returned by the provider.
        /// </summary>
        public string ProviderKey { get; }
    }
}
