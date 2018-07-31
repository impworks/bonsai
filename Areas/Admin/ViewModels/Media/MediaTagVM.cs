using System;
using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// Information about a single tagged entity on the photo.
    /// </summary>
    public class MediaTagVM: IMapped
    {
        /// <summary>
        /// ID of the tagged entity (if specified).
        /// </summary>
        public Guid? PageId { get; set; }

        /// <summary>
        /// Title of the tagged entity (if no page is specified).
        /// </summary>
        public string ObjectTitle { get; set; }

        /// <summary>
        /// Semicolon-separated coordinates of the tag.
        /// </summary>
        public string Coordinates { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<MediaTag, MediaTagVM>()
                   .MapMember(x => x.ObjectTitle, x => x.ObjectTitle)
                   .MapMember(x => x.Coordinates, x => x.Coordinates)
                   .MapMember(x => x.PageId, x => x.ObjectId);
        }
    }
}
