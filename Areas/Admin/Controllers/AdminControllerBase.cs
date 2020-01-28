using Bonsai.Areas.Admin.Logic.Auth;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Helpers;
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

        /// <summary>
        /// Handles OperationExceptions.
        /// </summary>
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
                context.ExceptionHandled = true;
                return;
            }

            base.OnActionExecuted(context);
        }

        /// <summary>
        /// Saves the message for the next page.
        /// </summary>
        protected ActionResult RedirectToSuccess(string msg)
        {
            ShowMessage(msg);
            return Redirect(DefaultActionUrl);
        }

        /// <summary>
        /// Displays a one-time message on the next page.
        /// </summary>
        protected void ShowMessage(string msg, bool success = true)
        {
            Session.Set(new OperationResultMessage
            {
                IsSuccess = success,
                Message = msg
            });
        }
    }
}
