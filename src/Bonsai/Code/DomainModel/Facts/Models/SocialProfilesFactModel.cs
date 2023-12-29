using System;
using System.ComponentModel;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// Template for specifying known social profile links.
    /// </summary>
    public class SocialProfilesFactModel: FactListModelBase<SocialProfileFactItem>
    {
        public override void Validate()
        {
            for (var i = 0; i < Values.Length; i++)
            {
                var item = Values[i];
                if (!Enum.IsDefined(item.Type))
                    throw new ValidationException(nameof(Page.Facts), $"Профиль #{i + 1}: тип не указан");

                if (item.Value?.StartsWith("https://") != true)
                    throw new ValidationException(nameof(Page.Facts), $"Профиль #{i + 1}: ссылка должна начинаться с 'https://'");
            }
        }
    }

    /// <summary>
    /// Single social profile link.
    /// </summary>
    public class SocialProfileFactItem
    {
        /// <summary>
        /// Type of the profile.
        /// </summary>
        public SocialProfileType Type { get; set; }

        /// <summary>
        /// Link to the profile.
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Known social profile links (to display icons properly).
    /// </summary>
    public enum SocialProfileType
    {
        [Description("Facebook")]
        Facebook,

        [Description("Twitter")]
        Twitter,

        [Description("Одноклассники")]
        Odnoklassniki,

        [Description("Вконтакте")]
        Vkontakte,

        [Description("Telegram")]
        Telegram,

        [Description("Youtube")]
        Youtube,

        [Description("Github")]
        Github,
    }
}
