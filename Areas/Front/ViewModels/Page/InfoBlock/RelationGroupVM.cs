using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Page.InfoBlock
{
    /// <summary>
    /// Information about a group of relations.
    /// </summary>
    public class RelationGroupVM
    {
        /// <summary>
        /// Title of the relations group.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Relations in the group.
        /// </summary>
        public ICollection<RelationVM> Relations { get; set; }
    }
}
