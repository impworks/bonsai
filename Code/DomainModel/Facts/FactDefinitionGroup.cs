using System.Collections.Generic;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// A group of correlated facts.
    /// </summary>
    public class FactDefinitionGroup
    {
        public FactDefinitionGroup(string id, string title, bool isMain, params IFactDefinition[] facts)
        {
            Id = id;
            Title = title;
            IsMain = isMain;
            Facts = facts;
        }

        /// <summary>
        /// Unique ID of the group.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Readable title of the group.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Flag indicating that this group should be shown at the top.
        /// </summary>
        public bool IsMain { get; }

        /// <summary>
        /// Nested fact definitions.
        /// </summary>
        public IReadOnlyList<IFactDefinition> Facts { get; }
    }
}
