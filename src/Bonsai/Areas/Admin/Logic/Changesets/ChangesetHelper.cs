using System;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.Logic.Changesets;

/// <summary>
/// Helper changeset-related methods used in multiple places.
/// </summary>
public static class ChangesetHelper
{
    /// <summary>
    /// Returns the changeset type.
    /// </summary>
    public static ChangesetType GetChangeType(object prev, object next, Guid? revertedId)
    {
        if (revertedId != null)
            return ChangesetType.Restored;

        if (prev == null)
            return ChangesetType.Created;

        if (next == null)
            return ChangesetType.Removed;

        return ChangesetType.Updated;
    }
}