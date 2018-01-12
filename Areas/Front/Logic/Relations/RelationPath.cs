using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// A single path in the definition relation.
    /// </summary>
    public class RelationPath
    {
        public RelationPath(string rawPath)
        {
            IsExcluded = rawPath[0] == '-';
            rawPath = rawPath.TrimStart('+', '-');

            IsBound = rawPath[0] == '!';
            rawPath = rawPath.TrimStart('!');

            Segments = rawPath.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Select(x => new RelationPathSegment(x))
                              .ToList();
        }

        /// <summary>
        /// Flag indicating that the current path segment must be excluded from the resulting selection.
        /// </summary>
        public bool IsExcluded { get; }

        /// <summary>
        /// Flag indicating that the path is bound by a particular pre-defined node (spouse-based grouping).
        /// </summary>
        public bool IsBound { get; }

        /// <summary>
        /// Required path segments.
        /// </summary>
        public IReadOnlyList<RelationPathSegment> Segments;
    }
}
