using Bonsai.Areas.Admin.Logic.Auth;
using Bonsai.Code.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Base class for all admin controllers.
    /// </summary>
    [Area("Admin")]
    [Authorize(Policy = AdminAuthRequirement.Name)]
    public abstract class AdminControllerBase: AppControllerBase
    {
    }
}
