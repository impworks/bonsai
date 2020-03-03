using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Bonsai.Code.Infrastructure;

namespace Bonsai.Areas.Admin.ViewModels.DynamicConfig
{
    /// <summary>
    /// Configuration properties.
    /// </summary>
    public class UpdateDynamicConfigVM: IMapped
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

        /// <summary>
        /// Flag indicating that new registrations are accepted.
        /// </summary>
        public bool AllowRegistration { get; set; }

        /// <summary>
        /// Tree render thoroughness coefficient.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Значение должно быть в диапазоне от 1 до 100")]
        public int TreeRenderThoroughness { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<UpdateDynamicConfigVM, Code.Services.Config.DynamicConfig>();
            profile.CreateMap<Code.Services.Config.DynamicConfig, UpdateDynamicConfigVM>();
        }
    }
}
