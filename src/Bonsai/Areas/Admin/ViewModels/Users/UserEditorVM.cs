using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Data.Models;
using Mapster;

namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// VM for editing a user.
    /// </summary>
    public class UserEditorVM: RegisterUserVM
    {
        /// <summary>
        /// Surrogate user ID.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Flag indicating user's current validation state.
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// Assigned role.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// ID of the personal page.
        /// </summary>
        public Guid? PersonalPageId { get; set; }

        /// <summary>
        /// Flag indicating that the user is in lockout mode.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Configures automatic mapping.
        /// </summary>
        public override void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<UserEditorVM, AppUser>()
                  .Map(x => x.Birthday, x => x.Birthday)
                  .Map(x => x.FirstName, x => x.FirstName)
                  .Map(x => x.MiddleName, x => x.MiddleName)
                  .Map(x => x.LastName, x => x.LastName)
                  .Map(x => x.Email, x => x.Email)
                  .Map(x => x.NormalizedEmail, x => x.Email.ToUpperInvariant())
                  .Map(x => x.PageId, x => x.PersonalPageId)
                  .Map(x => x.UserName, x => ClearEmail(x.Email))
                  .Map(x => x.NormalizedUserName, x => ClearEmail(x.Email).ToUpperInvariant());

            config.NewConfig<AppUser, UserEditorVM>()
                  .Map(x => x.Birthday, x => x.Birthday)
                  .Map(x => x.FirstName, x => x.FirstName)
                  .Map(x => x.MiddleName, x => x.MiddleName)
                  .Map(x => x.LastName, x => x.LastName)
                  .Map(x => x.Email, x => x.Email)
                  .Map(x => x.PersonalPageId, x => x.PageId)
                  .Map(x => x.IsLocked, x => x.LockoutEnabled && x.LockoutEnd > DateTimeOffset.Now)
                  .IgnoreNonMapped(true);
        }

        private static string ClearEmail(string email) => Regex.Replace(email, "[^a-z0-9]", "");
    }
}
