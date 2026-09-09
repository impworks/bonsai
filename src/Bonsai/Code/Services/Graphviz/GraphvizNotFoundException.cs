using System;

namespace Bonsai.Code.Services.Graphviz;

/// <summary>
/// Thrown when the "dot" executable cannot be launched at all.
/// This is a configuration problem rather than a problem with a particular
/// graph, so there is no point in trying the next graph: everything that
/// catches per-graph render errors must let this one through.
/// </summary>
public class GraphvizNotFoundException(string path, Exception inner)
    : Exception($"Failed to launch Graphviz (\"{path}\"). Make sure Graphviz is installed and \"dot\" is available on PATH.", inner)
{
    /// <summary>
    /// The path that was used to launch the executable.
    /// </summary>
    public string Path { get; } = path;
}
