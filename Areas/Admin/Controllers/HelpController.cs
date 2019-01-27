using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for displaying static guide pages.
    /// </summary>
    [Route("admin/help")]
    public class HelpController: AdminControllerBase
    {
        /// <summary>
        /// Displays the Markdown guide.
        /// </summary>
        [Route("markdown")]
        public ActionResult Markdown()
        {
            return View();
        }

        /// <summary>
        /// Displays the editor guidelines.
        /// </summary>
        [Route("guidelines")]
        public ActionResult Guidelines()
        {
            return View();
        }
    }
}
