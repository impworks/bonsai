using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.Logic.Relations.GroupFactories
{
    /// <summary>
    /// The factory for inferring relation to parents & siblings.
    /// </summary>
    public class ParentRelationGroupFactory: RelationGroupFactoryBase
    {
        public ParentRelationGroupFactory(string title, params RelationDefinition[] relations)
            : base(relations)
        {
            _title = title;
        }

        protected readonly string _title;

        /// <summary>
        /// Returns the related group.
        /// </summary>
        public override IEnumerable<RelationGroupVM> GetRelatedGroups(ICollection<Relation> relations)
        {
            yield break;
        }
    }
}
