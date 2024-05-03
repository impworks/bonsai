using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Code.Utils.Helpers;

/// <summary>
/// Helper methods for running external processes.
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    /// Invokes the process.
    /// </summary>
    public static async Task InvokeAsync(string file, string args, CancellationToken token = default)
    {
        using var proc = CreateProcess(file, args);
        await InvokeInternalAsync(proc, p => p.ExitCode, token);
    }

    /// <summary>
    /// Invokes the process and returns its output as a string.
    /// </summary>
    public static async Task<string> GetOutputAsync(string file, string args, CancellationToken token = default)
    {
        using var proc = CreateProcess(file, args);
        return await InvokeInternalAsync(proc, p => p.StandardOutput.ReadToEnd(), token);
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
    private static async Task<T> InvokeInternalAsync<T>(Process p, Func<Process, T> result, CancellationToken token)
    {
        p.Start();
        await p.WaitForExitAsync(token);
        p.WaitForExit(); // sic! this forces waiting until all output is ready
        return result(p);
    }

    #endregion
}