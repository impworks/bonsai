using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Page.InfoBlock
{
    /// <summary>
    /// Information about a list of relation groups.
    /// </summary>
    public class RelationCategoryVM
    {
        /// <summary>
        /// Title of the relations category.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Flag indicating that the category must be displayed at the top of the page.
        /// </summary>
        public bool IsMain { get; set; }

        /// <summary>
        /// List of nested groups.
        /// </summary>
        public IReadOnlyList<RelationGroupVM> Groups { get; set; }
    }
}
