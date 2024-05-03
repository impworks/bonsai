using System;
using System.ComponentModel.DataAnnotations;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;
using Mapster;

namespace Bonsai.Areas.Admin.ViewModels.Media;

/// <summary>
/// Editable details of a media file.
/// </summary>
public class MediaEditorVM: IMapped, IVersionable
{
    /// <summary>
    /// Surrogate ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of the media file.
    /// </summary>
    public MediaType Type { get; set; }

    /// <summary>
    /// Path to the file on the server.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Media creation date.
    /// </summary>
    [StringLength(30)]
    public string Date { get; set; }

    /// <summary>
    /// Title of the media (for documents).
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Markdown description of the media file.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Serialized tag info.
    /// </summary>
    public string DepictedEntities { get; set; }

    /// <summary>
    /// ID of the page or name of the location.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// ID of the page or name of the event.
    /// </summary>
    public string Event { get; set; }

    /// <summary>
    /// Action to execute on save.
    /// </summary>
    public MediaEditorSaveAction SaveAction { get; set; }

    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<Bonsai.Data.Models.Media, MediaEditorVM>()
              .Map(x => x.Id, x => x.Id)
              .Map(x => x.Type, x => x.Type)
              .Map(x => x.FilePath, x => x.FilePath)
              .Map(x => x.Date, x => x.Date)
              .Map(x => x.Title, x => x.Title)
              .Map(x => x.Description, x => x.Description)
              .IgnoreNonMapped(true);

        config.NewConfig<MediaEditorVM, Bonsai.Data.Models.Media>()
              .Map(x => x.Date, x => x.Date)
              .Map(x => x.Title, x => x.Title)
              .Map(x => x.Description, x => x.Description)
              .IgnoreNonMapped(true);
    }
}