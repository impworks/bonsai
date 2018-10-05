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
        public MediaController(MediaPresenterService media, CacheService cache)
        {
            _media = media;
            _cache = cache;
        }

        private readonly MediaPresenterService _media;
        private readonly CacheService _cache;

        /// <summary>
        /// Displays media and details.
        /// </summary>
        [Route("{key}")]
        public async Task<ActionResult> ViewMedia(string key)
        {
            var vm = await _cache.GetOrAddAsync(key, async() => await _media.GetMediaAsync(key));
            return View(vm);
        }
    }
}
