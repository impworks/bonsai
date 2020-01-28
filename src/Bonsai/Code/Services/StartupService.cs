using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// Helper service for managing startup tasks and dependencies.
    /// </summary>
    public class StartupService
    {
        public StartupService(ViewRenderService viewRender, ILogger logger)
        {
            _viewRender = viewRender;
            _logger = logger;
        }

        private readonly object _lockObject = new object();
        private readonly List<StartupTask> _workingTasks = new List<StartupTask>();
        private readonly ViewRenderService _viewRender;
        private readonly ILogger _logger;

        private Task _startupCompleted = Task.CompletedTask;

        /// <summary>
        /// Flag indicating that all startup actions have been completed.
        /// </summary>
        public bool IsStarted => _startupCompleted.IsCompleted;

        /// <summary>
        /// Completion task.
        /// </summary>
        public Task WaitForStartup() => _startupCompleted;

        /// <summary>
        /// Adds a new task to await before startup is completed.
        /// </summary>
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

        /// <summary>
        /// Displays the loading page until the startup is completed.
        /// </summary>
        public async Task RenderLoadingPage(HttpContext context, Func<Task> nextDelegate)
        {
            if (_startupCompleted.IsCompleted)
            {
                await nextDelegate();
                return;
            }

            var vm = _workingTasks.Where(x => !string.IsNullOrEmpty(x.Description));
            var body = await _viewRender.RenderToStringAsync("~/Areas/Front/Views/loading.cshtml", vm, context);
            await context.Response.WriteAsync(body);
        }
    }
}