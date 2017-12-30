using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// The root controller for pages.
    /// </summary>
    [Area("Front")]
    [Route("")]
    public class PageController: Controller
    {
        public PageController(PageService pages)
        {
            _pages = pages;
        }

        private readonly PageService _pages;

        /// <summary>
        /// Displays the page description.
        /// </summary>
        [Route("{key}")]
        public async Task<ActionResult> Description(string key)
        {
            var vm = await _pages.GetPageDescriptionAsync(key)
                                 .ConfigureAwait(false);
            return View(vm);
        }

        /// <summary>
        /// Displays the facts information.
        /// </summary>
        [Route("{key}/facts")]
        public async Task<ActionResult> Facts(string key)
        {
            var vm = await _pages.GetPageFactsAsync(key)
                                 .ConfigureAwait(false);
            return View(vm);
        }


        /// <summary>
        /// Displays the related media files.
        /// </summary>
        [Route("{key}/media")]
        public async Task<ActionResult> Media(string key)
        {
            var vm = await _pages.GetPageMediaAsync(key)
                                 .ConfigureAwait(false);
            return View(vm);
        }
    }
}
