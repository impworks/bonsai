using System;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils;

namespace Bonsai.Areas.Front.Logic.Relations
{
    public partial class RelationsPresenterService
    {
        /// <summary>
        /// Information about a page matching a relation path segment.
        /// </summary>
        private class RelationTarget: IEquatable<RelationTarget>
        {
            public readonly RelationContext.PageExcerpt Page;
            public readonly RelationContext.RelationExcerpt Relation;
            public readonly SinglyLinkedList<RelationContext.PageExcerpt> VisitedPages;

            public RelationTarget(RelationContext.PageExcerpt page, RelationContext.RelationExcerpt relation, SinglyLinkedList<RelationContext.PageExcerpt> visitedPages)
            {
                Page = page;
                Relation = relation;
                VisitedPages = visitedPages;
            }

            #region Equality members (auto-generated)

            public bool Equals(RelationTarget other) => !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || Page.Equals(other.Page));
            public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((RelationTarget)obj));
            public override int GetHashCode() => Page.GetHashCode();

            #endregion
        }
    }
}
