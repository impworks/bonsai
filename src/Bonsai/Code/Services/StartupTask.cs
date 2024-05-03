using System;
using System.Threading.Tasks;
using Serilog;

namespace Bonsai.Code.Services;

public class StartupTask
{
    private const int MAX_RETRIES = 3;
        
    public StartupTask(ILogger logger, string name, string description, Func<Task> action)
    {
        _logger = logger;
        _attempt = 1;
        _action = action;

        Name = name;
        Description = description;
    }

    private readonly Func<Task> _action;
    private readonly ILogger _logger;
    private int _attempt;

    /// <summary>
    /// Internal name of the task.
    /// </summary>
    public string Name { get; }
            
    /// <summary>
    /// Readable description displayed at the loading screen.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Completion flag.
    /// </summary>
    public bool IsCompleted { get; private set; }
        
    /// <summary>
    /// Flag indicating that a task has failed.
    /// </summary>
    public bool IsFailed { get; private set; }

    /// <summary>
    /// Executes the task with specified error handling.
    /// </summary>
    public Task Execute()
    {
        return RetryAsync();
    }

    /// <summary>
    /// Performs an action with automatic retry on error.
    /// </summary>
    private async Task RetryAsync(int retryDelayBackoff = 1000)
    {
        try
        {
            await _action();
            IsCompleted = true;
        }
        catch (Exception ex)
        {
            if (_attempt == MAX_RETRIES)
            {
                IsFailed = true;
                throw;
            }
                
            _logger.Error(ex, $"Failed to execute {Name} at startup (attempt {_attempt}), retrying in {TimeSpan.FromMilliseconds(retryDelayBackoff).TotalSeconds} second(s)...");
            _attempt++;
            await Task.Delay(retryDelayBackoff);
            await RetryAsync((int) Math.Min(30000, retryDelayBackoff * 1.5));
        }
    }
}