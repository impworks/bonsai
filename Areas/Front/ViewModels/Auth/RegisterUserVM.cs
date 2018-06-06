using System.ComponentModel.DataAnnotations;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Information for registering a new user.
    /// </summary>
    public class RegisterUserVM
    {
        /// <summary>
        /// The email address.
        /// </summary>
        [StringLength(255)]
        [Required(ErrorMessage = "Введите адрес электронной почты.")]
        [EmailAddress(ErrorMessage = "Введите валидный адрес электронной почты.")]
        public string Email { get; set; }

        /// <summary>
        /// First name.
        /// </summary>
        [StringLength(256)]
        [Required(ErrorMessage = "Введите ваше имя.")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name.
        /// </summary>
        [StringLength(256)]
        [Required(ErrorMessage = "Введите вашу фамилию.")]
        public string LastName { get; set; }

        /// <summary>
        /// Middle name.
        /// </summary>
        [StringLength(256)]
        [Required(ErrorMessage = "Введите ваше отчество.")]
        public string MiddleName { get; set; }

        /// <summary>
        /// Birthday.
        /// </summary>
        [StringLength(10)]
        [Required(ErrorMessage = "Введите дату вашего рождения.")]
        [RegularExpression("[0-9]{4}\\.[0-9]{2}\\.[0-9]{2}", ErrorMessage = "Введите дату в формате ГГГГ.ММ.ДД")]
        public string Birthday { get; set; }
    }
}
