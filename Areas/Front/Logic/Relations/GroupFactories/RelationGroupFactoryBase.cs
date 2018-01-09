using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.Logic.Relations.GroupFactories
{
    /// <summary>
    /// Base class for relation group generators.
    /// </summary>
    public abstract class RelationGroupFactoryBase
    {
        protected RelationGroupFactoryBase(RelationDefinition[] relations)
        {
            _relations = relations;
        }

        protected readonly IReadOnlyList<RelationDefinition> _relations;

        /// <summary>
        /// Returns the list of relation groups inferred from a set of page's relations.
        /// </summary>
        public abstract IEnumerable<RelationGroupVM> GetRelatedGroups(ICollection<Relation> relations);
    }
}
