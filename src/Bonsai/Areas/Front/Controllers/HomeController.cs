using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
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
    [Authorize(Policy = AuthRequirement.Name)]
    public class HomeController : AppControllerBase
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
        /// </summary>
        [Route("")]
        public async Task<ActionResult> Index()
        {
            var vm = new HomeVM
            {
                LastUpdatedPages = await _pages.GetLastUpdatedPagesAsync(5),
                LastUploadedMedia = await _media.GetLastUploadedMediaAsync(10)
            };

            return View(vm);
        }
    }
}