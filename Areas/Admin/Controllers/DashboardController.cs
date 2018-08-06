using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for the default admin page.
    /// </summary>
    [Route("admin/home")]
    public class DashboardController: AdminControllerBase
    {
        public DashboardController(DashboardPresenterService dash)
        {
            _dash = dash;
        }

        private readonly DashboardPresenterService _dash;

        /// <summary>
        /// Displays the main page.
        /// </summary>
        [Route("")]
        public async Task<ActionResult> Index()
        {
            var vm = await _dash.GetDashboardAsync().ConfigureAwait(false);
            return View(vm);
        }
    }
}
