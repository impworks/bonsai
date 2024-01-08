using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;
using Mapster;

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
        /// Flag indicating that black ribbon should not be displayed on deceased relatives in tree view.
        /// </summary>
        public bool HideBlackRibbon { get; set; }

        /// <summary>
        /// Tree render thoroughness coefficient.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Значение должно быть в диапазоне от 1 до 100")]
        public int TreeRenderThoroughness { get; set; }

        /// <summary>
        /// Kinds of tree which should be rendered automatically.
        /// </summary>
        public TreeKind[] TreeKinds { get; set; }

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateDynamicConfigVM, Code.Services.Config.DynamicConfig>()
                  .Map(x => x.Title, x => x.Title)
                  .Map(x => x.AllowGuests, x => x.AllowGuests)
                  .Map(x => x.AllowRegistration, x => x.AllowRegistration)
                  .Map(x => x.TreeRenderThoroughness, x => x.TreeRenderThoroughness)
                  .Map(x => x.HideBlackRibbon, x => x.HideBlackRibbon)
                  .Map(x => x.TreeKinds, x => x.TreeKinds == null ? 0 : x.TreeKinds.Aggregate((TreeKind)0, (a, b) => a | b));

            config.NewConfig<Code.Services.Config.DynamicConfig, UpdateDynamicConfigVM>()
                  .Map(x => x.Title, x => x.Title)
                  .Map(x => x.AllowGuests, x => x.AllowGuests)
                  .Map(x => x.AllowRegistration, x => x.AllowRegistration)
                  .Map(x => x.TreeRenderThoroughness, x => x.TreeRenderThoroughness)
                  .Map(x => x.HideBlackRibbon, x => x.HideBlackRibbon)
                  .Map(x => x.TreeKinds, x => Enum.GetValues<TreeKind>().Where(y => x.TreeKinds.HasFlag(y)).ToArray());
        }
    }
}
