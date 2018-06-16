using System.Threading.Tasks;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// Displays the sortable header.
    /// </summary>
    public class ListHeaderComponent: ViewComponent
    {
        /// <summary>
        /// Renders the sortable header.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(string url, ListRequestVM request, string propName, string title)
        {
            var vm = new ListHeaderVM
            {
                Title = title
            };

            var cloneRequest = ListRequestVM.Clone(request);
            cloneRequest.Page = 0;

            if (cloneRequest.OrderBy == propName)
            {
                cloneRequest.OrderDescending = !cloneRequest.OrderDescending;
                vm.IsDescending = cloneRequest.OrderDescending;
            }
            else
            {
                cloneRequest.OrderBy = propName;
                cloneRequest.OrderDescending = false;
            }

            vm.Url = ListRequestHelper.GetUrl(url, cloneRequest);
            return View("~/Areas/Admin/Views/Components/ListHeader.cshtml", vm);
        }
    }
}
