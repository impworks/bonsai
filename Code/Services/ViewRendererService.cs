using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// A service that renders views to strings.
    /// </summary>
    public class ViewRenderService
    {
        #region Constructor

        public ViewRenderService(
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Fields

        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Public methods

        /// <summary>
        /// Renders a view to string.
        /// </summary>
        public async Task<string> RenderToStringAsync(string viewName, object model, HttpContext httpContext = null)
        {
            if(httpContext == null)
                httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
 
            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.GetView("", viewName, false);
 
                if (viewResult.View == null)
                    throw new ArgumentNullException($"{viewName} does not match any available view.");
 
                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };
 
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );
 
                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }

        #endregion
    }
}
