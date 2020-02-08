using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// General application configuration.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Surrogate key.
        /// </summary>
        public Guid Id { get; set; }

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
    }
}
