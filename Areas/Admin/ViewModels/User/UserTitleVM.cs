using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.User
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
        /// The user's role with richest access level.
        /// </summary>
        public UserRole Role { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<AppUser, UserTitleVM>()
                   .ForMember(x => x.Role, opt => opt.Ignore())
                   .ForMember(x => x.FullName, opt => opt.MapFrom(y => y.FirstName + " " + y.LastName));
        }
    }
}
