using System;
using System.ComponentModel.DataAnnotations;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;
using Mapster;

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

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<Relation, PageRelationVM>()
                  .TwoWays()
                  .Map(x => x.SourceId, x => x.SourceId)
                  .Map(x => x.DestinationId, x => x.DestinationId)
                  .Map(x => x.Type, x => x.Type)
                  .Map(x => x.Duration, x => x.Duration)
                  .Map(x => x.EventId, x => x.EventId)
                  .IgnoreNonMapped(true);
        }
    }
}
