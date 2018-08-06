using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Admin.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// Renders the hidden filter components.
    /// </summary>
    public class ListHiddenFilterComponent: ViewComponent
    {
        /// <summary>
        /// Renders the included fields.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(ListRequestVM request, string[] include = null)
        {
            var values = ListRequestHelper.GetValues(request)
                                          .Where(x => x.Key == nameof(ListRequestVM.OrderBy)
                                                      || x.Key == nameof(ListRequestVM.OrderDescending)
                                                      || include?.Contains(x.Key) == true)
                                          .ToList();

            return View("~/Areas/Admin/Views/Components/ListHiddenFilter.cshtml", values);
        }
    }
}
