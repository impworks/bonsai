using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// Renders the filter for a related entity.
    /// </summary>
    public class ListItemFilterComponent: ViewComponent
    {
        /// <summary>
        /// Renders the filter item.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(string url, ListRequestVM request, string propName, string title)
        {
            if (request == null || propName == null || title == null)
                throw new ArgumentNullException();

            var prop = request.GetType().GetProperty(propName);
            if (prop == null)
                throw new ArgumentException($"Request of type '{request.GetType().Name}' does not have a property '{propName}'.");

            var cloneRequest = ListRequestVM.Clone(request);
            prop.SetValue(cloneRequest, prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null);

            var vm = new ListItemFilterVM
            {
                Title = title,
                CancelUrl = ListRequestHelper.GetUrl(url, cloneRequest)
            };

            return View("~/Areas/Admin/Views/Components/ListItemFilter.cshtml", vm);
        }
    }
}
