using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Home;
using Bonsai.Code.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// Main controller that displays a basic page.
    /// </summary>
    [Route("")]
    [Area("Front")]
    [Authorize]
    [Authorize(Policy = AuthRequirement.Name)]
    public class HomeController : Controller
    {
        public HomeController(PagePresenterService pages, MediaPresenterService media)
        {
            _pages = pages;
            _media = media;
        }

        private readonly PagePresenterService _pages;
        private readonly MediaPresenterService _media;

        /// <summary>
        /// Returns the main page.
        /// It is currently empty.
        /// </summary>
        [Route("")]
        public async Task<ActionResult> Index()
        {
            var count = 5;
            var lastPages = await _pages.GetLastUpdatedPagesAsync(count)
                                        .ConfigureAwait(false);
            var lastMedia = await _media.GetLastUploadedMediaAsync(count)
                                        .ConfigureAwait(false);

            var vm = new HomeVM
            {
                LastUpdatedPages = lastPages,
                LastUploadedMedia = lastMedia
            };

            return View(vm);
        }
    }
}