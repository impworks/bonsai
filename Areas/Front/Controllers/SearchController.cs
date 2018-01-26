using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// The controller for searching for pages.
    /// </summary>
    [Route("s")]
    public class SearchController: Controller
    {
        public SearchController(SearchPresenterService search)
        {
            _search = search;
        }

        private readonly SearchPresenterService _search;

        /// <summary>
        /// Returns the search results.
        /// </summary>
        [HttpGet]
        public async Task<ActionContext> Search([FromQuery] string query)
        {
            var vm = await _search.SearchAsync(query).ConfigureAwait(false);
            return View(vm);
        }
    }
}
