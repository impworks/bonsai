namespace Bonsai.Areas.Admin.ViewModels.Components
{
    /// <summary>
    /// Data for rendering a list's header.
    /// </summary>
    public class ListHeaderVM
    {
        /// <summary>
        /// Header rendered title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Header's new sort URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// False = ascending sort.
        /// True = descending sort.
        /// Null = not sorted by this field.
        /// </summary>
        public bool? IsDescending { get; set; }
    }
}
