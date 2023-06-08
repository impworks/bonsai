using System;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;
using Mapster;

namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// Brief information about the user.
    /// </summary>
    public class UserTitleVM: IMapped
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Readable name of the user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Flag indicating that the user is locally authorized with a login and password.
        /// </summary>
        public AuthType AuthType { get; set; }

        /// <summary>
        /// The user's role with richest access level.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Date of the current lockout.
        /// </summary>
        public DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// ID of the related page (if any).
        /// </summary>
        public Guid? PageId { get; set; }

        /// <summary>
        /// Flag indicating the user's gender.
        /// </summary>
        public bool? IsMale { get; set; }

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<AppUser, UserTitleVM>()
                  .Map(x => x.Id, x => x.Id)
                  .Map(x => x.Email, x => x.Email)
                  .Map(x => x.FullName, y => y.FirstName + " " + y.LastName)
                  .Map(x => x.AuthType, x => x.AuthType)
                  .Map(x => x.LockoutEnd, x => x.LockoutEnd)
                  .Map(x => x.PageId, x => x.PageId)
                  .Ignore(x => x.Role, x => x.IsMale);
        }
    }
}
