using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Relations
{
    /// <summary>
    /// A list of allowed relation types between two types of pages.
    /// </summary>
    public class RelationBinding
    {
        public RelationBinding(PageType sourceType, PageType destinationType, RelationType[] relTypes)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            RelationTypes = relTypes;
        }

        /// <summary>
        /// Type of the source page.
        /// </summary>
        public PageType SourceType { get; }

        /// <summary>
        /// Type of the destination page.
        /// </summary>
        public PageType DestinationType { get; }

        /// <summary>
        /// Type of the relation.
        /// </summary>
        public RelationType[] RelationTypes { get; }
    }
}
