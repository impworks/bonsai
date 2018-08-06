using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// Renders the page numbers for a list.
    /// </summary>
    public class ListPaginatorComponent: ViewComponent
    {
        /// <summary>
        /// Renders the pagination control.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(string url, ListRequestVM request, int pageCount)
        {
            var result = new List<ListPaginatorPageVM>();
            var pages = GetPageNumbers(request.Page, pageCount);
            var prevPage = (int?) null;

            foreach (var page in pages)
            {
                if(prevPage != null && prevPage != page - 1)
                    result.Add(new ListPaginatorPageVM { Title = "..." });

                var clone = ListRequestVM.Clone(request);
                clone.Page = page;

                var cloneUrl = ListRequestHelper.GetUrl(url, clone);
                result.Add(new ListPaginatorPageVM
                {
                    Url = cloneUrl,
                    Title = (page + 1).ToString(),
                    IsCurrent = page == request.Page
                });

                prevPage = page;
            }

            return View("~/Areas/Admin/Views/Components/ListPaginator.cshtml", result);
        }

        /// <summary>
        /// Returns the page IDs.
        /// </summary>
        private IEnumerable<int> GetPageNumbers(int current, int count)
        {
            const int Margin = 2;

            return Enumerable.Range(0, Margin)
                             .Concat(Enumerable.Range(current - Margin, Margin * 2 + 1))
                             .Concat(Enumerable.Range(count - Margin, Margin))
                             .Where(x => x >= 0 && x < count)
                             .Distinct();
        }
    }
}
