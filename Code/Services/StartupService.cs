using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace Bonsai.Code.Services
{
    public class StartupService
    {
        private readonly object _lockObject = new object();
        private readonly List<StartupTask> _workingTasks = new List<StartupTask>();
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly ILogger _logger;

        private Task _startupCompleted = Task.CompletedTask;

        public StartupService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider, ILogger logger)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _logger = logger;
        }

        public bool IsStarted => _startupCompleted.IsCompleted;

        public Task WaitForStartup() => _startupCompleted;

        public StartupTask RegisterStartupTask(string taskName, string description, Func<Task> task)
        {
            lock (_lockObject)
            {
                var startupTask = new StartupTask(_logger, this, taskName, description, task);
                _workingTasks.Add(startupTask);
                
                _startupCompleted = Task.WhenAll(_startupCompleted, startupTask.Task);
                return startupTask;
            }
        }

        public async Task LoadingPage(HttpContext context, Func<Task> nextDelegate)
        {
            if (_startupCompleted.IsCompleted)
            {
                await nextDelegate();
            }
            else
            {
                var startupView = _razorViewEngine.GetView("~/", "~/Areas/Front/Views/loading.cshtml", false);
                var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = _workingTasks
                };

                var sb = new StringBuilder();
                using var sw = new StringWriter(sb);
                
                var viewContext = new ViewContext(
                    actionContext,
                    startupView.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider), 
                    sw, 
                    new HtmlHelperOptions()
                );
 
                await startupView.View.RenderAsync(viewContext);
                await context.Response.WriteAsync(sb.ToString());
            }
        }
    }
}