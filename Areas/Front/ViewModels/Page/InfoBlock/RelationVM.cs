using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Page.InfoBlock
{
    /// <summary>
    /// Information about a particular relation.
    /// </summary>
    public class RelationVM
    {
        /// <summary>
        /// Readable title of the relation.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// List of related pages.
        /// </summary>
        public ICollection<RelatedPageVM> Pages { get; set; }
    }
}
