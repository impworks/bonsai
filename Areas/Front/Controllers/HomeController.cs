using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// Main controller that displays a basic page.
    /// </summary>
    [Route("")]
    [Area("Front")]
    public class HomeController : Controller
    {
        public HomeController(PagePresenterService pages, MediaPresenterService media, CalendarPresenterService calendar)
        {
            _pages = pages;
            _media = media;
            _calendar = calendar;
        }

        private readonly PagePresenterService _pages;
        private readonly MediaPresenterService _media;
        private readonly CalendarPresenterService _calendar;

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

        /// <summary>
        /// Returns the page for the calendar.
        /// </summary>
        [Route("~/util/cal/{year:int}/{month:int}")]
        public async Task<ActionResult> Calendar(int year, int month)
        {
            var vm = await _calendar.GetMonthEventsAsync(year, month)
                                    .ConfigureAwait(false);

            return View(vm);
        }
    }
}