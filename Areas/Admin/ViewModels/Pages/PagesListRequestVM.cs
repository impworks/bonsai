namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Search query options.
    /// </summary>
    public class PagesListRequestVM
    {
        /// <summary>
        /// Search query for page names.
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// Ordering field.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// Ordering direction.
        /// </summary>
        public bool OrderDescending { get; set; }

        /// <summary>
        /// Current page (0-based).
        /// </summary>
        public int Page { get; set; }
    }
}
