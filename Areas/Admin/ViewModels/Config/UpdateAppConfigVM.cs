using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Config
{
    /// <summary>
    /// Configuration properties.
    /// </summary>
    public class UpdateAppConfigVM: IMapped
    {
        /// <summary>
        /// The title of the website. Displayed in the top bar and browser title.
        /// </summary>
        [Required(ErrorMessage = "Введите название для заголовка сайта")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string Title { get; set; }

        /// <summary>
        /// Flag indicating that the website allows unauthorized visitors to view the contents.
        /// </summary>
        public bool AllowGuests { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<UpdateAppConfigVM, AppConfig>()
                   .ForMember(x => x.Id, opt => opt.Ignore());

            profile.CreateMap<AppConfig, UpdateAppConfigVM>();
        }
    }
}
