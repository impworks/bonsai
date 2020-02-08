using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Code.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// Controller for displaying error messages.
    /// </summary>
    [Route("error")]
    [Area("Front")]
    public class ErrorController: AppControllerBase
    {
        public ErrorController(AuthService auth)
        {
            _auth = auth;
        }

        private readonly AuthService _auth;

        /// <summary>
        /// Displays the "not found"
        /// </summary>
        [Route("404")]
        [HttpGet]
        public async Task<ActionResult> NotFoundError()
        {
            ViewBag.User = await _auth.GetCurrentUserAsync(User);
            return View("NotFound");
        }
    }
}
