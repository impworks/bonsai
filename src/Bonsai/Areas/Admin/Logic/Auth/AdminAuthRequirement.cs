using Microsoft.AspNetCore.Authorization;

namespace Bonsai.Areas.Admin.Logic.Auth
{
    /// <summary>
    /// Requirement for administrator access.
    /// </summary>
    public class AdminAuthRequirement: IAuthorizationRequirement
    {
        public const string Name = "AdminAuthRequirement";
    }
}
