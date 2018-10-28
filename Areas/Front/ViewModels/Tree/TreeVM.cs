using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Tree
{
    /// <summary>
    /// Entire contents of the family tree.
    /// </summary>
    public class TreeVM
    {
        /// <summary>
        /// All available spouse relations.
        /// </summary>
        public IReadOnlyList<TreeRelationVM> Relations { get; set; }

        /// <summary>
        /// All known persons.
        /// </summary>
        public IReadOnlyList<TreePersonVM> Persons { get; set; }

        /// <summary>
        /// ID of the person to center the view.
        /// </summary>
        public string RootId { get; set; }
    }
}
