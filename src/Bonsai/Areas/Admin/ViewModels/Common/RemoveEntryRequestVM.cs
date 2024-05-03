using System;

namespace Bonsai.Areas.Admin.ViewModels.Common;

/// <summary>
/// Request to remove an entity (page, media).
/// </summary>
public class RemoveEntryRequestVM
{
    /// <summary>
    /// ID of the removed entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Flag to remove all underlying data and related changesets.
    /// </summary>
    public bool RemoveCompletely { get; set; }
}