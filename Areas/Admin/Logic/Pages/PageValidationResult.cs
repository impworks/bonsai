using System;
using System.Collections.Generic;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.Logic.Pages
{
    /// <summary>
    /// Result of the new relation set's validation.
    /// </summary>
    public class PageValidationResult
    {
        /// <summary>
        /// Deserialized relations.
        /// </summary>
        public IReadOnlyList<Relation> Relations { get; set; }

        /// <summary>
        /// The list of page IDs affected by current change.
        /// </summary>
        public IReadOnlyList<Guid> AffectedPageIds { get; set; }
    }
}
