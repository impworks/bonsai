using System.Threading.Tasks;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Components
{
    public class OperationMessageComponent: ViewComponent
    {
        /// <summary>
        /// Displays the notification.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = HttpContext.Session.Get<OperationResultMessage>();
            if (vm == null)
                return Content("");

            HttpContext.Session.Remove<OperationResultMessage>();
            return View("~/Areas/Admin/Views/Components/OperationMessage.cshtml", vm);
        }
    }
}
