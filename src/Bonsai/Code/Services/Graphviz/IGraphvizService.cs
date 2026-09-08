using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Code.Services.Graphviz;

/// <summary>
/// Lays out DOT graphs.
/// </summary>
public interface IGraphvizService
{
    /// <summary>
    /// Lays out several DOT sources in one go and returns one result per source,
    /// in the order the sources were given.
    /// </summary>
    Task<IReadOnlyList<GraphvizLayout>> RenderAsync(IReadOnlyList<string> sources, CancellationToken token);
}
