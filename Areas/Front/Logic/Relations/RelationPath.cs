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
            Segments = rawPath.TrimStart('+', '-')
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Select(x => new RelationPathSegment(x))
                              .ToList();
        }

        /// <summary>
        /// Flag indicating that the current path segment must be excluded from the resulting selection.
        /// </summary>
        public bool IsExcluded { get; }

        /// <summary>
        /// Required path segments.
        /// </summary>
        public IReadOnlyList<RelationPathSegment> Segments;
    }
}
