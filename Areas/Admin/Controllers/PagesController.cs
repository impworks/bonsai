using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for handling pages.
    /// </summary>
    [Area("Admin")]
    [Route("admin/pages")]
    public class PagesController: AdminControllerBase
    {
        public PagesController(PagesManagerService pages)
        {
            _pages = pages;
        }

        public readonly PagesManagerService _pages;

        /// <summary>
        /// Displays the list of pages.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(ListRequestVM request)
        {
            var vm = await _pages.GetPagesAsync(request).ConfigureAwait(false);
            return View(vm);
        }
    }
}
