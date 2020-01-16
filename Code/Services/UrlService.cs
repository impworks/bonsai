using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// Wrapper service for providing access to IUrlHelper to other services.
    /// </summary>
    public class UrlService
    {
        public UrlService(IUrlHelperFactory urlFactory, IHttpContextAccessor httpAcc)
        {
            var actionCtx = new ActionContext(httpAcc.HttpContext, httpAcc.HttpContext.GetRouteData(), new ActionDescriptor());
            UrlHelper = urlFactory.GetUrlHelper(actionCtx);
        }

        public IUrlHelper UrlHelper { get; }
    }
}
