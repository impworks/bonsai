namespace Bonsai.Code.DomainModel.Relations
{
    /// <summary>
    /// Options for loading the RelationContext.
    /// </summary>
    public class RelationContextOptions
    {
        /// <summary>
        /// Loads only pages of type "Person" and their relations.
        /// </summary>
        public bool PeopleOnly { get; set; }

        /// <summary>
        /// Omits loading relations.
        /// </summary>
        public bool PagesOnly { get; set; }
    }
}
