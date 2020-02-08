using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Information for registering a new user.
    /// </summary>
    public class RegisterUserVM: IMapped
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

        /// <summary>
        /// Password to use when registering without external auth.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Copy of the password.
        /// </summary>
        public string PasswordCopy { get; set; }

        /// <summary>
        /// Flag indicating that the user must be granted a page.
        /// </summary>
        public bool CreatePersonalPage { get; set; }

        /// <summary>
        /// Configures Automapper maps.
        /// </summary>
        public virtual void Configure(IProfileExpression profile)
        {
            profile.CreateMap<RegisterUserVM, AppUser>()
                   .ForMember(x => x.Birthday, opt => opt.MapFrom(x => x.Birthday))
                   .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.FirstName))
                   .ForMember(x => x.MiddleName, opt => opt.MapFrom(x => x.MiddleName))
                   .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.LastName))
                   .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email))
                   .ForMember(x => x.UserName, opt => opt.MapFrom(x => Regex.Replace(x.Email, "[^a-z0-9]", "")))
                   .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
