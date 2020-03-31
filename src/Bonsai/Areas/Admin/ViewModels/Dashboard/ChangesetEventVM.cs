using System;
using System.Collections.Generic;
using AutoMapper;
using Bonsai.Areas.Admin.ViewModels.Changesets;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Users;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard
{
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

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Changeset, ChangesetEventVM>()
                   .ForMember(x => x.EntityType, opt => opt.MapFrom(x => x.Type))
                   .ForMember(x => x.ChangeType, opt => opt.ResolveUsing(GetChangesetType))
                   .ForMember(x => x.Date, opt => opt.MapFrom(x => x.Date))
                   .ForMember(x => x.User, opt => opt.MapFrom(x => x.Author))
                   .Ignore(x => x.ElementCount)
                   .Ignore(x => x.MediaThumbnails)
                   .Ignore(x => x.MainLink)
                   .Ignore(x => x.ExtraLinks);

            ChangesetType GetChangesetType(Changeset chg)
            {
                if (chg.RevertedChangesetId != null)
                    return ChangesetType.Restored;

                if (chg.OriginalState == null)
                    return ChangesetType.Created;

                if (chg.UpdatedState == null)
                    return ChangesetType.Removed;

                return ChangesetType.Updated;
            }
        }
    }
}
