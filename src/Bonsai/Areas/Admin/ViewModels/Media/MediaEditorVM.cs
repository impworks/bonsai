using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
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

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Bonsai.Data.Models.Media, MediaEditorVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.FilePath, x => x.FilePath)
                   .MapMember(x => x.Date, x => x.Date)
                   .MapMember(x => x.Title, x => x.Title)
                   .MapMember(x => x.Description, x => x.Description)
                   .Ignore(x => x.DepictedEntities)
                   .Ignore(x => x.Location)
                   .Ignore(x => x.Event)
                   .Ignore(x => x.SaveAction)
                   .ReverseMap()
                   .Ignore(x => x.Id)
                   .Ignore(x => x.Type)
                   .Ignore(x => x.FilePath);
        }
    }
}
