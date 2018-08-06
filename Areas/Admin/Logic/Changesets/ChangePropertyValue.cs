namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// Property name to value binding.
    /// </summary>
    public class ChangePropertyValue
    {
        public ChangePropertyValue(string title, string value)
        {
            Title = title;
            Value = value;
        }

        /// <summary>
        /// Name of the property.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Rendered value.
        /// </summary>
        public string Value { get; }
    }
}
