namespace Bonsai.Areas.Admin.ViewModels.Menu
{
    /// <summary>
    /// Element of the menu.
    /// </summary>
    public class MenuItemVM
    {
        /// <summary>
        /// Displayed title of the element.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL of the element.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Flag indicating that the element is selected.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
