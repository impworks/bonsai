using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// VM for creating a new password-authorized user.
    /// </summary>
    public class UserCreatorVM : IPasswordForm, IMapped
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
        [Required(ErrorMessage = "Введите имя пользователя.")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name.
        /// </summary>
        [StringLength(256)]
        [Required(ErrorMessage = "Введите фамилию пользователя.")]
        public string LastName { get; set; }

        /// <summary>
        /// Middle name.
        /// </summary>
        [StringLength(256)]
        [Required(ErrorMessage = "Введите отчество пользователя.")]
        public string MiddleName { get; set; }

        /// <summary>
        /// Birthday.
        /// </summary>
        [StringLength(10)]
        [Required(ErrorMessage = "Введите дату рождения пользователя.")]
        [RegularExpression("[0-9]{4}\\.[0-9]{2}\\.[0-9]{2}", ErrorMessage = "Введите дату в формате ГГГГ.ММ.ДД")]
        public string Birthday { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Password copy for typo checking.
        /// </summary>
        public string PasswordCopy { get; set; }

        /// <summary>
        /// Assigned role.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// ID of the personal page.
        /// </summary>
        public Guid? PersonalPageId { get; set; }

        /// <summary>
        /// Configures automatic mapping.
        /// </summary>
        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<UserCreatorVM, AppUser>()
                   .MapMember(x => x.Birthday, x => x.Birthday)
                   .MapMember(x => x.FirstName, x => x.FirstName)
                   .MapMember(x => x.MiddleName, x => x.MiddleName)
                   .MapMember(x => x.LastName, x => x.LastName)
                   .MapMember(x => x.Email, x => x.Email)
                   .MapMember(x => x.PageId, x => x.PersonalPageId)
                   .ForMember(x => x.UserName, opt => opt.MapFrom(x => Regex.Replace(x.Email, "[^a-z0-9]", "")))
                   .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
