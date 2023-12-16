using System.ComponentModel;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// Template for specifying known social profile links.
    /// </summary>
    public class SocialProfilesFactModel: FactListModelBase<SocialProfileFactItem>
    {
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
