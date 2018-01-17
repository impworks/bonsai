using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bonsai.Code.Tools;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// The internal classes for handling relations.
    /// </summary>
    public partial class RelationsPresenterService
    {
        /// <summary>
        /// Basic information about a page.
        /// </summary>
        [DebuggerDisplay("{Title} ({Id})")]
        private class PageExcerpt : IEquatable<PageExcerpt>
        {
            public Guid Id { get; set; }

            public string Title { get; set; }
            public string Key { get; set; }
            public PageType PageType { get; set; }
            public bool? Gender { get; set; }
            public FuzzyDate? BirthDate { get; set; }
            public FuzzyDate? DeathDate { get; set; }
            public string ShortName { get; set; }

            #region Equality members (auto-generated)

            public bool Equals(PageExcerpt other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id.Equals(other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PageExcerpt)obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public static bool operator ==(PageExcerpt left, PageExcerpt right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(PageExcerpt left, PageExcerpt right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        /// <summary>
        /// Basic information about a relation between two pages.
        /// </summary>
        [DebuggerDisplay("{Type}: {SourceId} -> {DestinationId} ({Duration})")]
        public class RelationExcerpt
        {
            public Guid SourceId { get; set; }
            public Guid DestinationId { get; set; }
            public RelationType Type { get; set; }
            public FuzzyRange? Duration { get; set; }
        }

        /// <summary>
        /// Information about a page matching a relation path segment.
        /// </summary>
        private class RelationTarget : IEquatable<RelationTarget>
        {
            public readonly PageExcerpt Page;
            public readonly RelationExcerpt Relation;
            public readonly SinglyLinkedList<PageExcerpt> VisitedPages;

            public RelationTarget(PageExcerpt page, RelationExcerpt relation, SinglyLinkedList<PageExcerpt> visitedPages)
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

        /// <summary>
        /// Information about all known pages and relations.
        /// </summary>
        private class RelationContext
        {
            public IReadOnlyDictionary<Guid, PageExcerpt> Pages;
            public IReadOnlyDictionary<Guid, IReadOnlyList<RelationExcerpt>> Relations;
        }
    }
}
