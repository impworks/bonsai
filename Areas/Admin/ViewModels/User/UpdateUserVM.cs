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
        public string Id { get; set; }

        /// <summary>
        /// Assigned role.
        /// </summary>
        public UserRole? Role { get; set; }

        /// <summary>
        /// Flag indicating that the user must be granted a page.
        /// </summary>
        public bool? CreatePersonalPage { get; set; }

        /// <summary>
        /// Configures automatic mapping.
        /// </summary>
        public override void Configure(IProfileExpression profile)
        {
            profile.CreateMap<UpdateUserVM, AppUser>()
                   .IncludeBase<RegisterUserVM, AppUser>()
                   .ForAllOtherMembers(x => x.Ignore());

            profile.CreateMap<AppUser, UpdateUserVM>()
                   .ForMember(x => x.Role, opt => opt.Ignore())
                   .ForMember(x => x.CreatePersonalPage, opt => opt.Ignore());
        }
    }
}
