using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Code.Facts
{
    /// <summary>
    /// Blueprint of a group of facts.
    /// </summary>
    public class FactGroupDefinition
    {
        public FactGroupDefinition(string key, string title, params FactDefinition[] facts)
        {
            Key = key;
            Title = title;
            Facts = facts.ToDictionary(x => x.Key, x => x);
        }

        /// <summary>
        /// Serialization key for the fact group.
        /// </summary>
        public string Key { get; protected set; }

        /// <summary>
        /// Title of the fact group.
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        /// The facts contained by current group.
        /// </summary>
        public IReadOnlyDictionary<string, FactDefinition> Facts { get; protected set; }
    }
}
