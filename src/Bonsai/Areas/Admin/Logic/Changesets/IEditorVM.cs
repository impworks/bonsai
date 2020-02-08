using System;

namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// Common interface for viewmodels that can be stored as changesets.
    /// </summary>
    public interface IVersionable
    {
        /// <summary>
        /// ID of the entity.
        /// </summary>
        Guid Id { get; }
    }
}
