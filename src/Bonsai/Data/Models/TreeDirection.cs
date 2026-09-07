namespace Bonsai.Data.Models;

/// <summary>
/// The vertical direction in which a tree is laid out.
/// </summary>
public enum TreeDirection
{
    /// <summary>
    /// Parents on top, children below them.
    /// </summary>
    TopToBottom = 0,

    /// <summary>
    /// Children on top, parents below them.
    /// </summary>
    BottomToTop = 1
}
