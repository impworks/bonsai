using System;
using System.Collections.Generic;

namespace Bonsai.Areas.Admin.ViewModels.Tree
{
    /// <summary>
    /// Contents of a family subtree.
    /// </summary>
    public class TreeLayoutVM
    {
        /// <summary>
        /// Related page (for partial trees only).
        /// </summary>
        public Guid? PageId { get; set; }

        /// <summary>
        /// All available spouse relations.
        /// </summary>
        public IReadOnlyList<TreeRelationVM> Relations { get; set; }

        /// <summary>
        /// All known persons.
        /// </summary>
        public IReadOnlyList<TreePersonVM> Persons { get; set; }
    }
}
