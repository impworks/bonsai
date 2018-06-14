using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.User
{
    /// <summary>
    /// Information about a user removal request.
    /// </summary>
    public class RemoveUserVM: IMapped
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User's full name (for clarification).
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Mapper configuration.
        /// </summary>
        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<AppUser, RemoveUserVM>()
                   .ForMember(x => x.FullName, opt => opt.MapFrom(y => y.FirstName + " " + y.LastName));
        }
    }
}
