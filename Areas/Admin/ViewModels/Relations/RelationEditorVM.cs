using System;
using AutoMapper;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Relations
{
    /// <summary>
    /// Editable details of a relation.
    /// </summary>
    public class RelationEditorVM: IMapped, IVersionable
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the source page.
        /// </summary>
        public Guid[] SourceIds { get; set; }

        /// <summary>
        /// ID of the second page.
        /// </summary>
        public Guid? DestinationId { get; set; }

        /// <summary>
        /// ID of the related event.
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// Type of the relation.
        /// </summary>
        public RelationType Type { get; set; }

        /// <summary>
        /// Relation timespan's start.
        /// </summary>
        public string DurationStart { get; set; }

        /// <summary>
        /// Relation timespan's end.
        /// </summary>
        public string DurationEnd { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Relation, RelationEditorVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.SourceIds, x => new [] { x.SourceId })
                   .MapMember(x => x.DestinationId, x => x.DestinationId)
                   .MapMember(x => x.EventId, x => x.EventId)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.DurationStart, x => FuzzyRange.TrySplit(x.Duration)[0])
                   .MapMember(x => x.DurationEnd, x => FuzzyRange.TrySplit(x.Duration)[1])
                   .ReverseMap()
                   .MapMember(x => x.Duration, x => FuzzyRange.TryCombine(x.DurationStart, x.DurationEnd))
                   .MapMember(x => x.SourceId, x => x.SourceIds != null && x.SourceIds.Length > 0 ? x.SourceIds[0] : Guid.Empty);
        }
    }
}
