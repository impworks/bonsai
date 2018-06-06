using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for the default admin page.
    /// </summary>
    public class HomeController: AdminControllerBase
    {
        /// <summary>
        /// Displays the main page.
        /// </summary>
        public async Task<ActionResult> Index()
        {
            return View();
        }
    }
}
