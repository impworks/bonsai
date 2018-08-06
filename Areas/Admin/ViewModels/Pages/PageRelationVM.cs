using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Information about a relation.
    /// </summary>
    public class PageRelationVM: IMapped
    {
        /// <summary>
        /// ID of the source page.
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// ID of the second page.
        /// </summary>
        public Guid DestinationId { get; set; }

        /// <summary>
        /// ID of the related event.
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// Type of the relation.
        /// </summary>
        public RelationType Type { get; set; }

        /// <summary>
        /// Timespan of the relation.
        /// </summary>
        [StringLength(30)]
        public string Duration { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Relation, PageRelationVM>()
                   .MapMember(x => x.SourceId, x => x.SourceId)
                   .MapMember(x => x.DestinationId, x => x.DestinationId)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.Duration, x => x.Duration)
                   .MapMember(x => x.EventId, x => x.EventId)
                   .ReverseMap();
        }
    }
}
