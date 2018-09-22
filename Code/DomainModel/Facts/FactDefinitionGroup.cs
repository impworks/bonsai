using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// A group of correlated facts.
    /// </summary>
    public class FactDefinitionGroup
    {
        public FactDefinitionGroup(string id, string title, bool isMain, params IFactDefinition[] defs)
        {
            Id = id;
            Title = title;
            IsMain = isMain;
            Defs = defs;
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
        [JsonIgnore]
        public IReadOnlyList<IFactDefinition> Defs { get; }
    }
}
