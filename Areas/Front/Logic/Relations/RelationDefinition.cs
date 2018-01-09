using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// Definition of a single relationship between two pages.
    /// </summary>
    public class RelationDefinition
    {
        public RelationDefinition(string rawPath, string singularName, string pluralName = null, bool isRanged = false)
        {
            OriginalRawPath = rawPath;
            Path = rawPath.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => new RelationPathSegment(x)).ToList();
            SingularName = singularName;
            PluralName = pluralName;
            IsRanged = isRanged;
        }

        /// <summary>
        /// Path for current node.
        /// </summary>
        public string OriginalRawPath { get; }

        /// <summary>
        /// Parsed path.
        /// </summary>
        public IReadOnlyList<RelationPathSegment> Path { get; }

        /// <summary>
        /// Corresponding name for a single item.
        /// </summary>
        public string SingularName { get; }

        /// <summary>
        /// Corresponding name for a group of items.
        /// </summary>
        public string PluralName { get; }

        /// <summary>
        /// Flag indicating that the relation has an actuality range.
        /// </summary>
        public bool IsRanged { get; }
    }
}
