using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Relations
{
    public class RelationGroup
    {
        public RelationGroup(string title, params RelationType?[] types)
        {
            Title = title;
            Types = types;
        }

        /// <summary>
        /// Title of the group.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Contained relation types.
        /// </summary>
        public RelationType?[] Types { get; }
    }
}
