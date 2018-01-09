using System;
using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.Logic.Relations.GroupFactories
{
    /// <summary>
    /// Generates groups for each spouse and their respective families.
    /// </summary>
    public class SpouseRelationGroupFactory: RelationGroupFactoryBase
    {
        public SpouseRelationGroupFactory(params RelationDefinition[] relations)
            : base(relations)
        {
        }

        public override IEnumerable<RelationGroupVM> GetRelatedGroups(ICollection<Relation> relations)
        {
            yield break;
        }
    }
}
