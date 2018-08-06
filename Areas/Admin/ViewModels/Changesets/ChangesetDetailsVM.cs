using System;
using System.Collections.Generic;

namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// Base information about a changeset.
    /// </summary>
    public class ChangesetDetailsVM
    {
        /// <summary>
        /// ID of the changeset.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Edit date.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// User that has authored the edit.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Changed items.
        /// </summary>
        public IReadOnlyList<ChangeVM> Changes { get; set; }
    }
}
