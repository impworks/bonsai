using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.Logic.Relations.GroupFactories
{
    /// <summary>
    /// Returns the groups for non-human relations.
    /// </summary>
    public class OtherRelationGroupFactory: RelationGroupFactoryBase
    {
        public OtherRelationGroupFactory(params RelationDefinition[] relations)
            : base(relations)
        {
        }

        public override IEnumerable<RelationGroupVM> GetRelatedGroups(ICollection<Relation> relations)
        {
            yield break;
        }
    }
}
