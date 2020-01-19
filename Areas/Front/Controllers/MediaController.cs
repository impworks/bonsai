using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// The controller for displaying media information.
    /// </summary>
    [Area("Front")]
    [Route("m")]
    [Authorize(Policy = AuthRequirement.Name)]
    public class MediaController: AppControllerBase
    {
        public MediaController(MediaPresenterService media, CacheService cache, AuthService auth)
        {
            _media = media;
            _cache = cache;
            _auth = auth;
        }

        private readonly MediaPresenterService _media;
        private readonly CacheService _cache;
        private readonly AuthService _auth;

        /// <summary>
        /// Displays media and details.
        /// </summary>
        [Route("{key}")]
        public async Task<ActionResult> ViewMedia(string key)
        {
            var vm = await _cache.GetOrAddAsync(key, async() => await _media.GetMediaAsync(key));
            ViewBag.User = await _auth.GetCurrentUserAsync(User);
            return View(vm);
        }
    }
}
