using System;
using System.Threading.Tasks;
using Serilog;

namespace Bonsai.Code.Services
{
    public class StartupTask
    {
        private readonly ILogger _logger;
        private readonly StartupService _startupService;
        private readonly Task _task;

        public StartupTask(ILogger logger, StartupService startupService, string name, string description, Func<Task> task)
        {
            _logger = logger;
            _startupService = startupService;
            Name = name;
            Description = description;
            _task = RetryUntilSuccessAsync(task);
        }
            
        public string Name { get; }
            
        public string Description { get; }

        public Task Task => _task;

        public bool IsCompleted => _task.IsCompleted;
        
        private async Task RetryUntilSuccessAsync(Func<Task> retryableAction, int retryDelayBackoff = 1000)
        {
            try
            {
                await retryableAction();
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"Failed to perform {Name} at startup, retrying in {TimeSpan.FromMilliseconds(retryDelayBackoff).TotalSeconds} second...");
                await Task.Delay(retryDelayBackoff);
            }

            await RetryUntilSuccessAsync(retryableAction, (int) Math.Min(30000, retryDelayBackoff * 1.5));
        }


        public StartupTask ContinueWith(string taskName, string description, Func<Task> startupTask)
        {
            return _startupService.RegisterStartupTask(taskName, description, () => _task.ContinueWith(_ => startupTask()).Unwrap());
        }
    }
}