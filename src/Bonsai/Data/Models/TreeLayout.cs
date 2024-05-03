using System;

namespace Bonsai.Data.Models;

/// <summary>
/// Precalculated layout of the family tree.
/// </summary>
public class TreeLayout
{
    /// <summary>
    /// Surrogate ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Kind of the tree.
    /// </summary>
    public TreeKind Kind { get; set; }

    /// <summary>
    /// Related page (if the layout is for a specific page).
    /// </summary>
    public Page Page { get; set; }

    /// <summary>
    /// Id of the related page.
    /// </summary>
    public Guid? PageId { get; set; }

    /// <summary>
    /// Date of the layout's generation.
    /// </summary>
    public DateTimeOffset GenerationDate { get; set; }

    /// <summary>
    /// The rendered layout.
    /// </summary>
    public string LayoutJson { get; set; }
}