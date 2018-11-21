using System.Collections.Generic;
using Bonsai.Code.Utils.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bonsai.Code.Infrastructure
{
    /// <summary>
    /// Base class for all Bonsai-app controllers.
    /// </summary>
    public class AppControllerBase: Controller
    {
        /// <summary>
        /// Returns the current session.
        /// </summary>
        protected ISession Session => HttpContext.Session;

        /// <summary>
        /// Sets the state for the model from a validation exception.
        /// </summary>
        protected void SetModelState(ValidationException ex)
        {
            foreach(var error in ex.Errors)
                ModelState.AddModelError(error.Key, error.Value);
        }

        /// <summary>
        /// Handles OperationExceptions.
        /// </summary>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is KeyNotFoundException)
            {
                context.Result = NotFound();
                context.ExceptionHandled = true;
                return;
            }

            base.OnActionExecuted(context);
        }
    }
}
