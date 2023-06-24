using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Code.Services.Jobs
{
    /// <summary>
    /// Interface for job implementations.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Returns the resource key occupied by this job.
        /// </summary>
        string GetResourceKey(object args);
        
        /// <summary>
        /// Runs the job.
        /// </summary>
        Task ExecuteAsync(object args, CancellationToken token);
    }
}