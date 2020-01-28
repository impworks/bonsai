using Microsoft.AspNetCore.Authorization;

namespace Bonsai.Areas.Front.Logic.Auth
{
    /// <summary>
    /// Empty requirement class.
    /// Is validated by <see cref="AuthHandler" />.
    /// </summary>
    public class AuthRequirement: IAuthorizationRequirement
    {
        /// <summary>
        /// Name of the policy.
        /// </summary>
        public const string Name = "AuthRequirement";
    }
}
