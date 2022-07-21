using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services;
using Bonsai.Code.Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// Controller for displaying trees.
    /// </summary>
    [Area("Front")]
    [Route("tree")]
    [Authorize(Policy = AuthRequirement.Name)]
    public class TreeController : AppControllerBase
    {
        public TreeController(PagePresenterService pages, TreePresenterService tree, CacheService cache)
        {
            _pages = pages;
            _tree = tree;
            _cache = cache;
        }
        
        private readonly PagePresenterService _pages;
        private readonly TreePresenterService _tree;
        private readonly CacheService _cache;

        /// <summary>
        /// Displays the internal page for a tree.
        /// </summary>
        [Route("{key}/main")]
        public async Task<ActionResult> Main(string key)
        {
            var encKey = PageHelper.EncodeTitle(key);
            if (encKey != key)
                return RedirectToActionPermanent("Main", new { key = encKey });

            var model = await _cache.GetOrAddAsync(key, () => _pages.GetPageTreeAsync(key));

            return View(model);
        }
        
        /// <summary>
        /// Returns the rendered tree.
        /// </summary>
        [Route("~/util/tree/{key}")]
        public async Task<ActionResult> GetTreeData(string key)
        {
            var encKey = PageHelper.EncodeTitle(key);
            var data = await _tree.GetTreeAsync(encKey);
            return Json(data);
        }
    }
}