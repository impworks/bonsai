using Microsoft.AspNetCore.Authorization;

namespace Bonsai.Areas.Admin.Logic.Auth;

/// <summary>
/// Requirement for administrator-only access (users, global config).
/// </summary>
public class AdminOnlyAuthRequirement: IAuthorizationRequirement
{
    public const string Name = "AdminOnlyAuthRequirement";
}