using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Code.DomainModel.Facts.Models;

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
        /// The name fact (if specified).
        /// </summary>
        public NameFactModel Name { get; set; }

        /// <summary>
        /// Groups of inferred relations.
        /// </summary>
        public IReadOnlyCollection<RelationGroupVM> RelationGroups { get; set; }

        /// <summary>
        /// Facts bound to this particular person (besides name & photo).
        /// </summary>
        public IReadOnlyCollection<FactGroupVM> PersonalFacts { get; set; }
    }
}
