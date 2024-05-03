using System;

namespace Bonsai.Areas.Admin.ViewModels.Common;

/// <summary>
/// Request arguments for picking a page/media.
/// </summary>
public class PickRequestVM<T> where T: Enum
{
    /// <summary>
    /// Search query.
    /// </summary>
    public string Query { get; set; }

    /// <summary>
    /// Number of items to display.
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Number of items to skip (e.g. pagination).
    /// </summary>
    public int? Offset { get; set; }

    /// <summary>
    /// Types of entities to include (e.g. Person/Pet/etc for pages, Photo/Video for media).
    /// </summary>
    public T[] Types { get; set; }
}