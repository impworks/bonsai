namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Details of a scoring point for a page.
    /// </summary>
    public class PageScoreCriterionVM
    {
        /// <summary>
        /// Name of the scoring criterion.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Flag indicating that the criterion matched.
        /// </summary>
        public bool IsFilled { get; set; }
    }
}
