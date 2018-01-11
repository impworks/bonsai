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
        public string Duration { get; set; }
    }
}
