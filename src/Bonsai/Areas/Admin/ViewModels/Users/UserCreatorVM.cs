using System;
using System.Text.RegularExpressions;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Data.Models;
using Mapster;

namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// VM for creating a new password-authorized user.
    /// </summary>
    public class UserCreatorVM : RegisterUserVM, IPasswordForm
    {
        /// <summary>
        /// Assigned role.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// ID of the personal page.
        /// </summary>
        public Guid? PersonalPageId { get; set; }

        /// <summary>
        /// Configures automatic mapping.
        /// </summary>
        public override void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<UserCreatorVM, AppUser>()
                  .Map(x => x.Birthday, x => x.Birthday)
                  .Map(x => x.FirstName, x => x.FirstName)
                  .Map(x => x.MiddleName, x => x.MiddleName)
                  .Map(x => x.LastName, x => x.LastName)
                  .Map(x => x.Email, x => x.Email)
                  .Map(x => x.PageId, x => x.PersonalPageId)
                  .Map(x => x.UserName, x => Regex.Replace(x.Email, "[^a-z0-9]", ""))
                  .IgnoreNonMapped(true);
        }
    }
}
