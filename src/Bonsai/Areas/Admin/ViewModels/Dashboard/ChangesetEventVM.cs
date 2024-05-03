using System;
using System.Collections.Generic;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Users;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;
using Mapster;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard;

/// <summary>
/// Details of a changeset to be displayed in the dashboard view.
/// </summary>
public class ChangesetEventVM: IMapped
{
    /// <summary>
    /// Edit date.
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Type of the changed entity.
    /// </summary>
    public ChangesetEntityType EntityType { get; set; }

    /// <summary>
    /// Type of the change.
    /// </summary>
    public ChangesetType ChangeType { get; set; }

    /// <summary>
    /// Number of elements grouped in this change (only for MediaThumbnails for now).
    /// </summary>
    public int ElementCount { get; set; }

    /// <summary>
    /// Thumbnails for media (limited to 50).
    /// </summary>
    public IReadOnlyList<MediaThumbnailVM> MediaThumbnails { get; set; }

    /// <summary>
    /// Author of the change.
    /// </summary>
    public UserTitleVM User { get; set; }

    /// <summary>
    /// Link to the entity.
    /// </summary>
    public LinkVM MainLink { get; set; }

    /// <summary>
    /// Related links.
    /// </summary>
    public IReadOnlyList<LinkVM> ExtraLinks { get; set; }

    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<Changeset, ChangesetEventVM>()
              .Map(x => x.EntityType, x => x.EntityType)
              .Map(x => x.ChangeType, x => x.ChangeType)
              .Map(x => x.Date, x => x.Date)
              .Map(x => x.User, x => x.Author)
              .IgnoreNonMapped(true);
    }
}