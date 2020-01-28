namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// Property name to value binding.
    /// </summary>
    public class ChangePropertyValue
    {
        public ChangePropertyValue(string propertyName, string title, string value)
        {
            PropertyName = propertyName;
            Title = title;
            Value = value;
        }

        /// <summary>
        /// Original name of the property (as in source code).
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Readable of the property.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Rendered value.
        /// </summary>
        public string Value { get; }
    }
}
