using Bonsai.Areas.Admin.Logic.Auth;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Code.Mvc;
using Bonsai.Code.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Base class for all admin controllers.
    /// </summary>
    [Area("Admin")]
    [Authorize(Policy = AdminAuthRequirement.Name)]
    public abstract class AdminControllerBase: AppControllerBase
    {
        /// <summary>
        /// Name of the default action to use for redirection when an OperationException occurs.
        /// </summary>
        protected virtual string DefaultActionUrl => Url.Action("Index");

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is OperationException oe)
            {
                Session.Set(new OperationResultMessage
                {
                    IsSuccess = false,
                    Message = oe.Message
                });
                context.Result = Redirect(DefaultActionUrl);
                return;
            }

            base.OnActionExecuted(context);
        }

        /// <summary>
        /// Saves the message for the next page.
        /// </summary>
        protected ActionResult RedirectToSuccess(string msg)
        {
            Session.Set(new OperationResultMessage
            {
                IsSuccess = true,
                Message = msg
            });

            return Redirect(DefaultActionUrl);
        }
    }
}
