using System.Collections.Generic;
using Bonsai.Code.DomainModel.Facts.Models;

namespace Bonsai.Areas.Front.ViewModels
{
    /// <summary>
    /// The description section of a page.
    /// </summary>
    public class PageDescriptionVM: PageTitleVM
    {
        /// <summary>
        /// Main description (in HTML format).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The photo fact (if defined).
        /// </summary>
        public PhotoFactModel PhotoFact { get; set; }

        /// <summary>
        /// The name fact (if defined).
        /// </summary>
        public NameFactModel NameFact { get; set; }

        /// <summary>
        /// Facts bound to this particular person (besides name & photo).
        /// </summary>
        public ICollection<FactGroupVM> PersonalFacts { get; set; }

        /// <summary>
        /// Facts about the relation between this page and other pages.
        /// </summary>
        public ICollection<FactGroupVM> RelationFacts { get; set; }
    }
}
