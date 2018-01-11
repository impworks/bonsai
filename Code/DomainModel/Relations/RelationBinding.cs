using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Relations
{
    /// <summary>
    /// A list of allowed relation types between two types of pages.
    /// </summary>
    public class RelationBinding
    {
        public RelationBinding(PageType sourceType, PageType targetType, RelationType[] relTypes)
        {
            SourceType = sourceType;
            TargetType = targetType;
            RelationTypes = relTypes;
        }

        /// <summary>
        /// Type of the source page.
        /// </summary>
        public PageType SourceType { get; }

        /// <summary>
        /// Type of the destination page.
        /// </summary>
        public PageType TargetType { get; }

        /// <summary>
        /// Type of the relation.
        /// </summary>
        public RelationType[] RelationTypes { get; }
    }
}
