using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Code.Utils.Helpers;
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
            var vm = await _dash.GetDashboardAsync();
            return View(vm);
        }

        /// <summary>
        /// Returns the list of events at specified page.
        /// </summary>
        [Route("events")]
        public async Task<ActionResult> Events([FromQuery] int page = 0)
        {
            var vm = await _dash.GetEventsAsync(page).ToListAsync();
            return View(vm);
        }
    }
}
