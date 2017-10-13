namespace Bonsai.Code.Facts
{
    /// <summary>
    /// The blueprint for fact statements.
    /// </summary>
    public class FactDefinition
    {
        public FactDefinition(string key, string title, string templateName, bool isPermanent = true)
        {
            Key = key;
            Title = title;
            DisplayTemplatePath = $"~/Areas/Front/Views/Page/Facts/{templateName}.cshtml";
        }

        /// <summary>
        /// Fact key name for serialization.
        /// </summary>
        public string Key { get; protected set; }

        /// <summary>
        /// Readable fact name.
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        /// The path to template used for displaying the fact's contents.
        /// </summary>
        public string DisplayTemplatePath { get; protected set; }

        /// <summary>
        /// Flag indicating that the current fact is actual during the entire lifespan of the person.
        /// </summary>
        public bool IsPermanent { get; protected set; }
    }
}
