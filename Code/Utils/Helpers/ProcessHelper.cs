using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper methods for running external processes.
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Invokes the process.
        /// </summary>
        public static async Task InvokeAsync(string file, string args)
        {
            using (var proc = CreateProcess(file, args))
                await InvokeInternalAsync(proc, p => p.ExitCode);
        }

        /// <summary>
        /// Invokes the process and returns its output as a string.
        /// </summary>
        public static async Task<string> GetOutputAsync(string file, string args)
        {
            using(var proc = CreateProcess(file, args))
                return await InvokeInternalAsync(proc, p => p.StandardOutput.ReadToEnd());
        }

        #region Private helpers

        /// <summary>
        /// Creates a configures Process.
        /// </summary>
        private static Process CreateProcess(string file, string args)
        {
            return new Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    FileName = file,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        /// <summary>
        /// Wraps the process as a task.
        /// </summary>
        private static Task<T> InvokeInternalAsync<T>(Process p, Func<Process, T> result)
        {
            var tcs = new TaskCompletionSource<T>();
            p.Exited += (s, e) => tcs.SetResult(result(p));
            p.Start();
            return tcs.Task;
        }

        #endregion
    }
}
