using System.Collections.Generic;

namespace Bonsai.Areas.Admin.ViewModels.Tree
{
    /// <summary>
    /// Contents of a family subtree.
    /// </summary>
    public class TreeLayoutVM
    {
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
