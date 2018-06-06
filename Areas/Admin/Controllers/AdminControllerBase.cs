using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Base class for all admin controllers.
    /// </summary>
    [Area("Admin")]
    [Authorize]
    public abstract class AdminControllerBase: Controller
    {
    }
}
