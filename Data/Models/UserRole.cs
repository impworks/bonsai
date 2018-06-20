using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Known account roles.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Newly registered user.
        /// </summary>
        [Description("Новый")]
        Unvalidated,

        /// <summary>
        /// Basic user.
        /// </summary>
        [Description("Пользователь")]
        User,

        /// <summary>
        /// Page editor.
        /// </summary>
        [Description("Редактор")]
        Editor,

        /// <summary>
        /// Almighty administator
        /// </summary>
        [Description("Администратор")]
        Admin
    }
}
