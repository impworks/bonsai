using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// The root controller for pages.
    /// </summary>
    [Area("Front")]
    [Route("p")]
    [Authorize(Policy = AuthRequirement.Name)]
    public class PageController: AppControllerBase
    {
        public PageController(PagePresenterService pages, AuthService auth, CacheService cache)
        {
            _pages = pages;
            _auth = auth;
            _cache = cache;
        }

        private readonly PagePresenterService _pages;
        private readonly AuthService _auth;
        private readonly CacheService _cache;

        /// <summary>
        /// Displays the page description.
        /// </summary>
        [Route("{key}")]
        public async Task<ActionResult> Description(string key)
        {
            return await DisplayTab(key, () => _pages.GetPageDescriptionAsync(key));
        }

        /// <summary>
        /// Displays the related media files.
        /// </summary>
        [Route("{key}/media")]
        public async Task<ActionResult> Media(string key)
        {
            return await DisplayTab(key, () => _pages.GetPageMediaAsync(key));
        }

        /// <summary>
        /// Displays the tree pane.
        /// </summary>
        [Route("{key}/tree")]
        public async Task<ActionResult> Tree(string key)
        {
            return await DisplayTab(key, () => _pages.GetPageTreeAsync(key));
        }

        /// <summary>
        /// Displays the page tab.
        /// </summary>
        private async Task<ActionResult> DisplayTab<T>(string key, Func<Task<T>> bodyGetter, [CallerMemberName] string methodName = null)
            where T: PageTitleVM
        {
            var encKey = PageHelper.EncodeTitle(key);
            if (encKey != key)
                return RedirectToActionPermanent(methodName, new { key = encKey });

            try
            {
                ViewBag.User = await _auth.GetCurrentUserAsync(User);
                var vm = new PageVM<T>
                {
                    Body = await _cache.GetOrAddAsync(key, bodyGetter),
                    InfoBlock = await _cache.GetOrAddAsync(key, async () => await _pages.GetPageInfoBlockAsync(key))
                };
                return View(vm);
            }
            catch(RedirectRequiredException ex)
            {
                return RedirectToActionPermanent(methodName, new { key = ex.Key });
            }
        }
    }
}
