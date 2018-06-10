using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Code.Mvc
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
    }
}
