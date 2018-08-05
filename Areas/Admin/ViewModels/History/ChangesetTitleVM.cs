using System;
using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.History
{
    /// <summary>
    /// Brief information about a changeset.
    /// </summary>
    public class ChangesetTitleVM: IMapped
    {
        /// <summary>
        /// Changeset ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Edit date.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// Type of the change.
        /// </summary>
        public ChangesetType ChangeType { get; set; }

        /// <summary>
        /// Type of the changed entity.
        /// </summary>
        public ChangesetEntityType EntityType { get; set; }

        /// <summary>
        /// ID of the entity that has been edited.
        /// </summary>
        public Guid EntityId { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Changeset, ChangesetTitleVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Date, x => x.Date)
                   .MapMember(x => x.EntityType, x => x.Type)
                   .MapMember(x => x.EntityId, x => x.EntityId)
                   .MapMember(x => x.ChangeType, x => GetChangeType(x));
        }

        private static ChangesetType GetChangeType(Changeset chg)
        {
            var wasNull = string.IsNullOrEmpty(chg.OriginalState);
            var isNull = string.IsNullOrEmpty(chg.UpdatedState);

            if (wasNull)
                return ChangesetType.Created;

            if (isNull)
                return ChangesetType.Removed;

            return ChangesetType.Updated;
        }
    }
}
