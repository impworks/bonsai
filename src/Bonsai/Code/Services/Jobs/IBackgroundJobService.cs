using System.Threading.Tasks;

namespace Bonsai.Code.Services.Jobs;

/// <summary>
/// Public interface for handling background tasks.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Runs a new persistent background task.
    /// </summary>
    Task RunAsync(JobBuilder jb);
        
    /// <summary>
    /// Terminates all background tasks acquire the resource (specified by key).
    /// </summary>
    void Cancel(string key);
}