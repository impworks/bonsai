using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Code.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// Main controller that displays a basic page.
    /// </summary>
    [Route("")]
    [Area("Front")]
    [Authorize(Policy = AuthRequirement.Name)]
    public class HomeController : AppControllerBase
    {
        /// <summary>
        /// Returns the main page.
        /// </summary>
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }
    }
}