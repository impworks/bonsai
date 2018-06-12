using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AutoMapper;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.User
{
    /// <summary>
    /// VM for editing a user.
    /// </summary>
    public class UpdateUserVM: RegisterUserVM
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
        /// Flag indicating that the user must be granted a page.
        /// </summary>
        public bool CreatePersonalPage { get; set; }

        /// <summary>
        /// Configures automatic mapping.
        /// </summary>
        public override void Configure(IProfileExpression profile)
        {
            profile.CreateMap<UpdateUserVM, AppUser>()
                   .ForMember(x => x.Birthday, opt => opt.MapFrom(x => x.Birthday))
                   .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.FirstName))
                   .ForMember(x => x.MiddleName, opt => opt.MapFrom(x => x.MiddleName))
                   .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.LastName))
                   .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email))
                   .ForMember(x => x.UserName, opt => opt.ResolveUsing(x => Regex.Replace(x.Email, "[^a-z0-9]", "")))
                   .ForAllOtherMembers(x => x.Ignore());

            profile.CreateMap<AppUser, UpdateUserVM>()
                   .ForMember(x => x.Role, opt => opt.Ignore())
                   .ForMember(x => x.CreatePersonalPage, opt => opt.Ignore());
        }
    }
}
