using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for the default admin page.
    /// </summary>
    [Route("admin/home")]
    public class DashboardController: AdminControllerBase
    {
        /// <summary>
        /// Displays the main page.
        /// </summary>
        [Route("")]
        public async Task<ActionResult> Index()
        {
            return View();
        }
    }
}
