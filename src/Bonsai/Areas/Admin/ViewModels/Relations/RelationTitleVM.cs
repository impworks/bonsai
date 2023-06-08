using System;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Date;
using Bonsai.Data.Models;
using Mapster;

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

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<Relation, RelationTitleVM>()
                   .Map(x => x.Id, x => x.Id)
                   .Map(x => x.Type, x => x.Type)
                   .Map(x => x.Source, x => x.Source)
                   .Map(x => x.Destination, x => x.Destination)
                   .Map(x => x.Event, x => x.Event)
                   .Map(x => x.Duration, x => FuzzyRange.TryParse(x.Duration));
        }
    }
}
