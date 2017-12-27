namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// Shared interface for untyped fact definitions.
    /// </summary>
    public interface IFactDefinition
    {
        /// <summary>
        /// Unique ID for referencing the fact.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Readable title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Returns the path for display template.
        /// </summary>
        string ViewTemplatePath { get; }

        /// <summary>
        /// Returns the path for editor template.
        /// </summary>
        string EditTemplatePath { get; }
    }
}
