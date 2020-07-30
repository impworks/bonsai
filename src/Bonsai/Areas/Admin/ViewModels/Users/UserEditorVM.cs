using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AutoMapper;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

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
        public override void Configure(IProfileExpression profile)
        {
            profile.CreateMap<UserEditorVM, AppUser>()
                   .MapMember(x => x.Birthday, x => x.Birthday)
                   .MapMember(x => x.FirstName, x => x.FirstName)
                   .MapMember(x => x.MiddleName, x => x.MiddleName)
                   .MapMember(x => x.LastName, x => x.LastName)
                   .MapMember(x => x.Email, x => x.Email)
                   .MapMember(x => x.NormalizedEmail, x => x.Email.ToUpperInvariant())
                   .MapMember(x => x.PageId, x => x.PersonalPageId)
                   .ForMember(x => x.UserName, opt => opt.MapFrom(x => ClearEmail(x.Email)))
                   .ForMember(x => x.NormalizedUserName, opt => opt.MapFrom(x => ClearEmail(x.Email).ToUpperInvariant()))
                   .ForAllOtherMembers(x => x.Ignore());

            profile.CreateMap<AppUser, UserEditorVM>()
                   .MapMember(x => x.PersonalPageId, x => x.PageId)
                   .MapMember(x => x.IsLocked, x => x.LockoutEnabled && x.LockoutEnd > DateTimeOffset.Now)
                   .ForMember(x => x.Role, opt => opt.Ignore())
                   .ForMember(x => x.CreatePersonalPage, opt => opt.Ignore())
                   .ForMember(x => x.Password, opt => opt.Ignore())
                   .ForMember(x => x.PasswordCopy, opt => opt.Ignore());
        }

        private static string ClearEmail(string email) => Regex.Replace(email, "[^a-z0-9]", "");
    }
}
