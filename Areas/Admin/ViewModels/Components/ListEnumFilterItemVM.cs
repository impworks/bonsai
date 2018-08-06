namespace Bonsai.Areas.Admin.ViewModels.Components
{
    /// <summary>
    /// Displays the single filter item.
    /// </summary>
    public class ListEnumFilterItemVM
    {
        /// <summary>
        /// Name of the field.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Element's readable title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Element's underlying value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Flag indicating that the element is currently selected.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
