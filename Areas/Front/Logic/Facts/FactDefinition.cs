namespace Bonsai.Areas.Front.Logic.Facts
{
    /// <summary>
    /// Blueprint of a fact's template and editor.
    /// </summary>
    public class FactDefinition
    {
        public FactDefinition(string id, string title, FactTemplate template)
        {
            Id = id;
            Title = title;
            Template = template;
        }

        /// <summary>
        /// Unique ID for referencing the fact.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Readable title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// ID of the fact's template (used for both displaying and editing).
        /// </summary>
        public FactTemplate Template { get; }
    }
}
