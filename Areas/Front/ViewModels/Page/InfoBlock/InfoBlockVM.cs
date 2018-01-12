using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Media;

namespace Bonsai.Areas.Front.ViewModels.Page.InfoBlock
{
    /// <summary>
    /// The list of facts related to a particular page.
    /// </summary>
    public class InfoBlockVM
    {
        /// <summary>
        /// The photo (if specified).
        /// </summary>
        public MediaThumbnailVM Photo { get; set; }

        /// <summary>
        /// Facts about the person.
        /// </summary>
        public IReadOnlyCollection<FactGroupVM> Facts { get; set; }

        /// <summary>
        /// Groups of inferred relations.
        /// </summary>
        public IReadOnlyCollection<RelationCategoryVM> RelationGroups { get; set; }
    }
}
