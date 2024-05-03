using System;
using Bonsai.Areas.Admin.Logic.Auth;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.Controllers;

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
    protected ActionResult RedirectToSuccess(string msg = null)
    {
        if(!string.IsNullOrEmpty(msg))
            ShowMessage(msg);

        if (TempData[ListStateKey] is string listState)
        {
            try
            {
                var json = (ListRequestVM) JsonConvert.DeserializeObject(listState, ListStateType);
                var url = ListRequestHelper.GetUrl(DefaultActionUrl, json);
                return Redirect(url);
            }
            catch
            {
            }
        }
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

    /// <summary>
    /// Stores the last list state in the temporary storage.
    /// </summary>
    protected void PersistListState(ListRequestVM request)
    {
        TempData[ListStateKey] = JsonConvert.SerializeObject(request, Formatting.None);
    }

    /// <summary>
    /// Returns the key for persisting storage.
    /// </summary>
    private string ListStateKey => GetType().Name + "." + nameof(ListRequestVM);

    /// <summary>
    /// Default type to use for list state deserialization.
    /// </summary>
    protected virtual Type ListStateType => typeof(ListRequestVM);
}