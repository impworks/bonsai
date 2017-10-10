using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// Main controller that displays a basic page.
    /// </summary>
    [Route("")]
    [Area("Front")]
    public class HomeController: Controller
    {
        /// <summary>
        /// Returns the main page.
        /// It is currently empty.
        /// </summary>
        [Route("")]
        public ActionResult Index()
        {
            // todo: last updated pages, media, date calendar, etc.

            return View();
        }
    }
}
