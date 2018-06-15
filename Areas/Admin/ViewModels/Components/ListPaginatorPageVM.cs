namespace Bonsai.Areas.Admin.ViewModels.Components
{
    /// <summary>
    /// Element of a single page.
    /// </summary>
    public class ListPaginatorPageVM
    {
        /// <summary>
        /// Flag indicating that this page is selected.
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Displayed title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Link URL.
        /// </summary>
        public string Url { get; set; }
    }
}
