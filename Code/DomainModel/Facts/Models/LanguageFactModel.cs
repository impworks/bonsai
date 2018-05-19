using System.ComponentModel;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying known languages.
    /// </summary>
    public class LanguageFactModel: FactListModelBase<LanguageFactItem>
    {
    }

    /// <summary>
    /// Information about a single known language.
    /// </summary>
    public class LanguageFactItem : DurationFactItem
    {
        /// <summary>
        /// Name of the language.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Language proficiency.
        /// </summary>
        public LanguageProficiency? Proficiency { get; set; }
    }

    /// <summary>
    /// The proficiency in the specified language.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public enum LanguageProficiency
    {
        [Description("Базовый")]
        Beginner,

        [Description("Средний")]
        Intermediate,

        [Description("Свободный")]
        Profound,

        [Description("Родной")]
        Native
    }
}
