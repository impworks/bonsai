using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.ViewModels.Tree;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// The controller for displaying a family tree.
    /// </summary>
    [Area("front")]
    [Route("t")]
    [Authorize(Policy = AuthRequirement.Name)]
    public class TreeController: AppControllerBase
    {
        public TreeController(TreePresenterService tree, CacheService cache)
        {
            _tree = tree;
            _cache = cache;
        }

        private readonly TreePresenterService _tree;
        private readonly CacheService _cache;

        /// <summary>
        /// Returns the entire family tree's JSON.
        /// </summary>
        [HttpGet]
        [Route("{key}")]
        public async Task<TreeVM> Tree(string key)
        {
            return await _cache.GetOrAddAsync(key, async () =>
            {
                var tree = await _tree.GetTreeAsync(key);

                foreach (var person in tree.Persons)
                {
                    if(!string.IsNullOrEmpty(person.Url))
                        person.Url = Url.Action("Description", "Page", new {area = "Front", key = person.Url});

                    if (!string.IsNullOrEmpty(person.Photo))
                        person.Photo = Url.Content(person.Photo);
                }

                return tree;
            });
        }
    }
}
