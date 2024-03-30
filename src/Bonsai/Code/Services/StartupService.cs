using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// Helper service for managing startup tasks and dependencies.
    /// </summary>
    public class StartupService
    {
        public StartupService(IServiceProvider services, ILogger logger)
        {
            _services = services;
            _logger = logger;

            _tcs = new TaskCompletionSource();
            _workingTasks = new List<StartupTask>();
        }

        private readonly TaskCompletionSource _tcs;
        private readonly List<StartupTask> _workingTasks;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        /// <summary>
        /// Flag indicating that all startup actions have been completed.
        /// </summary>
        public bool IsStarted => _tcs.Task.IsCompleted;

        /// <summary>
        /// Completion task.
        /// </summary>
        public Task WaitForStartup() => _tcs.Task;

        /// <summary>
        /// Adds a new task to await before startup is completed.
        /// </summary>
        public void AddTask(string taskName, string description, Func<Task> task)
        {
            lock (_workingTasks)
            {
                var startupTask = new StartupTask(_logger, taskName, description, task);
                _workingTasks.Add(startupTask);
            }
        }

        /// <summary>
        /// Executes startup tasks. 
        /// </summary>
        public void RunStartupTasks()
        {
            Task.Run(async () =>
            {
                try
                {
                    foreach (var task in _workingTasks)
                        await task.Execute();

                    _tcs.SetResult();
                }
                catch
                {
                    // all logged
                }
            });
        }

        /// <summary>
        /// Displays the loading page until the startup is completed.
        /// </summary>
        public async Task RenderLoadingPage(HttpContext context, Func<Task> nextDelegate)
        {
            if (_tcs.Task.IsCompleted)
            {
                await nextDelegate();
                return;
            }

            using var scope = _services.CreateScope();
            var viewRender = scope.ServiceProvider.GetRequiredService<ViewRenderService>();
            var vm = _workingTasks.Where(x => !string.IsNullOrEmpty(x.Description));
            var body = await viewRender.RenderToStringAsync("~/Areas/Front/Views/loading.cshtml", vm, context);
            await context.Response.WriteAsync(body);
        }
    }
}