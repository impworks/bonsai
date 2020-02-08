using Bonsai.Code.Utils.Date;

namespace Bonsai.Areas.Front.ViewModels.Page.InfoBlock
{
    /// <summary>
    /// A single related page.
    /// </summary>
    public class RelatedPageVM: PageTitleVM
    {
        /// <summary>
        /// Range of the relation (if applicable).
        /// </summary>
        public FuzzyRange? Duration { get; set; }

        /// <summary>
        /// Page of the event bound to the relation.
        /// </summary>
        public PageTitleVM RelationEvent { get; set; }
    }
}
