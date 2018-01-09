using System;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// A single segment of the path.
    /// </summary>
    public class RelationPathSegment
    {
        public RelationPathSegment(string part)
        {
            var sep = part.IndexOf(':');
            if (sep == -1)
            {
                Type = Enum.Parse<RelationType>(part, true);
            }
            else
            {
                Type = Enum.Parse<RelationType>(part.Substring(0, sep), true);
                Gender = part[sep + 1] == 'm';
            }
        }

        /// <summary>
        /// Expected type of the direct relation between pages.
        /// </summary>
        public readonly RelationType Type;

        /// <summary>
        /// Flag indicating the expected gender (if it is required).
        /// </summary>
        public readonly bool? Gender;
    }
}
