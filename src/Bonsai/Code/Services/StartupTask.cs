using System;
using System.Threading.Tasks;
using Serilog;

namespace Bonsai.Code.Services
{
    public class StartupTask
    {
        public StartupTask(ILogger logger, StartupService startupService, string name, string description, Func<Task> task)
        {
            _logger = logger;
            _startupService = startupService;

            Name = name;
            Description = description;
            Task = RetryUntilSuccessAsync(task);
        }

        private readonly ILogger _logger;
        private readonly StartupService _startupService;

        /// <summary>
        /// Internal name of the task.
        /// </summary>
        public string Name { get; }
            
        /// <summary>
        /// Readable description displayed at the loading screen.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Awaitable task.
        /// </summary>
        public Task Task { get; }

        /// <summary>
        /// Completion flag.
        /// </summary>
        public bool IsCompleted => Task.IsCompleted;

        /// <summary>
        /// Chains a new task to be completed after the current one.
        /// </summary>
        public StartupTask ContinueWith(string taskName, string description, Func<Task> startupTask)
        {
            return _startupService.RegisterStartupTask(taskName, description, () => Task.ContinueWith(_ => startupTask()).Unwrap());
        }

        /// <summary>
        /// Performs an action with automatic retry on error.
        /// </summary>
        private async Task RetryUntilSuccessAsync(Func<Task> retryableAction, int retryDelayBackoff = 1000)
        {
            try
            {
                await retryableAction();
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to perform {Name} at startup, retrying in {TimeSpan.FromMilliseconds(retryDelayBackoff).TotalSeconds} second(s)...");
                await Task.Delay(retryDelayBackoff);
            }

            await RetryUntilSuccessAsync(retryableAction, (int) Math.Min(30000, retryDelayBackoff * 1.5));
        }
    }
}