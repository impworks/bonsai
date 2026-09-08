using System.Collections.Generic;

namespace Bonsai.Code.Services.Graphviz;

/// <summary>
/// The part of a Graphviz layout the tree builder actually needs: the canvas
/// height and where each node landed.
/// </summary>
public class GraphvizLayout
{
    /// <summary>
    /// Height of the drawing, in points.
    /// </summary>
    public double Height { get; init; }

    /// <summary>
    /// Node centres by node name, in points, measured from the bottom left.
    /// </summary>
    public IReadOnlyDictionary<string, GraphvizPoint> Nodes { get; init; }
}

/// <summary>
/// A point in Graphviz coordinates.
/// </summary>
public readonly record struct GraphvizPoint(double X, double Y);
