using System.ComponentModel.DataAnnotations;

namespace Bonsai.Code.Services.Config
{
    /// <summary>
    /// General application configuration.
    /// </summary>
    public class DynamicConfig
    {
        /// <summary>
        /// The title of the website. Displayed in the top bar and browser title.
        /// </summary>
        [Required]
        [StringLength(200)]
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
        public int TreeRenderThoroughness { get; set; }
    }
}
