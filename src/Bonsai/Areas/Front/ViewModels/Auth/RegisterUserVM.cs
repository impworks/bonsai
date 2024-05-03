using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;
using Bonsai.Localization;
using Mapster;

namespace Bonsai.Areas.Front.ViewModels.Auth;

/// <summary>
/// Information for registering a new user.
/// </summary>
public class RegisterUserVM: IMapped
{
    /// <summary>
    /// The email address.
    /// </summary>
    [StringLength(255)]
    [Required(ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = "Front_Register_Validation_EmailEmpty")]
    [EmailAddress(ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = "Front_Register_Validation_EmailInvalid")]
    public string Email { get; set; }

    /// <summary>
    /// First name.
    /// </summary>
    [StringLength(256)]
    [Required(ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = "Front_Register_Validation_FirstNameEmpty")]
    public string FirstName { get; set; }

    /// <summary>
    /// Last name.
    /// </summary>
    [StringLength(256)]
    [Required(ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = "Front_Register_Validation_LastNameEmpty")]
    public string LastName { get; set; }

    /// <summary>
    /// Middle name.
    /// </summary>
    [StringLength(256)]
    public string MiddleName { get; set; }

    /// <summary>
    /// Birthday.
    /// </summary>
    [StringLength(10)]
    [Required(ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = "Front_Register_Validation_BirthdayEmpty")]
    [RegularExpression("[0-9]{4}\\.[0-9]{2}\\.[0-9]{2}", ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = "Front_Register_Validation_BirthdayInvalid")]
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
    public virtual void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterUserVM, AppUser>()
              .Map(x => x.Birthday, x => x.Birthday)
              .Map(x => x.FirstName, x => x.FirstName)
              .Map(x => x.MiddleName, x => x.MiddleName)
              .Map(x => x.LastName, x => x.LastName)
              .Map(x => x.Email, x => x.Email)
              .Map(x => x.UserName, x => Regex.Replace(x.Email, "[^a-z0-9]", ""))
              .IgnoreNonMapped(true);
    }
}