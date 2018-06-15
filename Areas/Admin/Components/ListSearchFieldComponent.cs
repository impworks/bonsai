using System.Threading.Tasks;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// The component for rendering a list's search form.
    /// </summary>
    public class ListSearchFieldComponent: ViewComponent
    {
        /// <summary>
        /// Renders the search field.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(string url, ListRequestVM request)
        {
            var values = ListRequestHelper.GetValues(request);
            values.Remove(nameof(request.SearchQuery));

            var vm = new ListSearchFieldVM
            {
                Url = url,
                SearchQuery = request?.SearchQuery ?? "",
                OtherValues = values
            };

            return View("~/Areas/Admin/Views/Components/ListSearchField.cshtml", vm);
        }
    }
}
