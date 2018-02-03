using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// The controller for searching for pages.
    /// </summary>
    [Area("front")]
    [Route("search")]
    public class SearchController: Controller
    {
        public SearchController(SearchPresenterService search)
        {
            _search = search;
        }

        private readonly SearchPresenterService _search;

        [HttpGet]
        [Route("~/util/search/{*query}")]
        public async Task<IReadOnlyList<PageTitleVM>> Autocomplete(string query)
        {
            var hints = await _search.SearchAutocompleteAsync(query).ConfigureAwait(false);
            return hints;
        }

        /// <summary>
        /// Returns the search results.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Search([FromQuery] string query, [FromQuery] int page = 0)
        {
            page = Math.Max(0, page);

            var results = await _search.SearchAsync(query, page).ConfigureAwait(false);
            var vm = new SearchVM {Query = query, Page = page, Results = results};
            return View(vm);
        }
    }
}
