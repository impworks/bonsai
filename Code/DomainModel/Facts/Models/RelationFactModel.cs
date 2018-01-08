using Bonsai.Code.Tools;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying a relation between two entities.
    /// </summary>
    public class RelationFactModel: FactModelBase
    {
        /// <summary>
        /// Readable description of the relation.
        /// </summary>
        public string RelationTitle { get; set; }

        /// <summary>
        /// Name of the related person.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Page key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Duration of the relation.
        /// </summary>
        public FuzzyRange? Range { get; set; }
    }
}
