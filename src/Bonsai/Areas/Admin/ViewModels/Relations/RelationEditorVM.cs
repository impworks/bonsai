using System;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Date;
using Bonsai.Data.Models;
using Mapster;

namespace Bonsai.Areas.Admin.ViewModels.Relations;

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

    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<Relation, RelationEditorVM>()
              .Map(x => x.Id, x => x.Id)
              .Map(x => x.SourceIds, x => new[] {x.SourceId})
              .Map(x => x.DestinationId, x => x.DestinationId)
              .Map(x => x.EventId, x => x.EventId)
              .Map(x => x.Type, x => x.Type)
              .Map(x => x.DurationStart, x => FuzzyRange.TrySplit(x.Duration)[0])
              .Map(x => x.DurationEnd, x => FuzzyRange.TrySplit(x.Duration)[1]);

        config.NewConfig<RelationEditorVM, Relation>()
              .Map(x => x.Duration, x => FuzzyRange.TryCombine(x.DurationStart, x.DurationEnd))
              .Map(x => x.SourceId, x => x.SourceIds != null && x.SourceIds.Length > 0 ? x.SourceIds[0] : Guid.Empty);
    }
}