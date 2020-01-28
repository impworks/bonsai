using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// Definition of a single relationship between two pages.
    /// </summary>
    public class RelationDefinition
    {
        public RelationDefinition(string rawPaths, string singularNames, string pluralName = null, RelationDurationDisplayMode? durationMode = null)
        {
            _singularNames = singularNames.Split('|');
            _pluralName = pluralName;

            RawPaths = rawPaths;
            DurationDisplayMode = durationMode;
            Paths = Regex.Split(rawPaths, "(?=[+-])").Select(x => new RelationPath(x)).ToList();
        }

        private readonly string[] _singularNames;
        private readonly string _pluralName;

        /// <summary>
        /// Path for current node.
        /// </summary>
        public string RawPaths { get; }

        /// <summary>
        /// Parsed path.
        /// </summary>
        public IReadOnlyList<RelationPath> Paths { get; }

        /// <summary>
        /// The mode for displaying a range.
        /// </summary>
        public RelationDurationDisplayMode? DurationDisplayMode { get; }

        /// <summary>
        /// Returns the corresponding name for a possibly unspecified gender.
        /// </summary>
        public string GetName(int count, bool? isMale)
        {
            if (count > 1)
                return _pluralName;

            if (isMale == null && _singularNames.Length > 2)
                return _singularNames[2];

            if (isMale == false && _singularNames.Length > 1)
                return _singularNames[1];

            return _singularNames[0];
        }
    }
}
