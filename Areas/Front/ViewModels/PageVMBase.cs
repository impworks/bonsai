namespace Bonsai.Areas.Front.ViewModels
{
    /// <summary>
    /// Base view model for all page sections.
    /// </summary>
    public class PageVMBase
    {
        /// <summary>
        /// Title of the page (displayed in the header).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Key of the page (url-friendly version of the title).
        /// </summary>
        public string Key { get; set; }
    }
}
