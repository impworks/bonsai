using System;
using AutoMapper;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Relations
{
    /// <summary>
    /// Brief information about a relation between two pages.
    /// </summary>
    public class RelationTitleVM: IMapped
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The first entity in the relation.
        /// </summary>
        public PageTitleExtendedVM Source { get; set; }

        /// <summary>
        /// The second entity in the relation.
        /// </summary>
        public PageTitleExtendedVM Destination { get; set; }

        /// <summary>
        /// Related event (e.g. wedding).
        /// </summary>
        public PageTitleExtendedVM Event { get; set; }

        /// <summary>
        /// Type of the relation.
        /// </summary>
        public RelationType Type { get; set; }

        /// <summary>
        /// Timespan of the relation.
        /// </summary>
        public FuzzyRange? Duration { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Relation, RelationTitleVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMemberDynamic(x => x.Source, x => x.Source)
                   .MapMemberDynamic(x => x.Destination, x => x.Destination)
                   .MapMemberDynamic(x => x.Event, x => x.Event)
                   .MapMember(x => x.Duration, x => FuzzyRange.TryParse(x.Duration));
        }
    }
}
