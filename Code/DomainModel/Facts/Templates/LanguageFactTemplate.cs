using System.ComponentModel;

namespace Bonsai.Code.DomainModel.Facts.Templates
{
    /// <summary>
    /// The template for specifying known languages.
    /// </summary>
    public class LanguageFactTemplate: IFactTemplate
    {
        public LanguageFactTemplateItem[] Values { get; set; }
    }

    public class LanguageFactTemplateItem : RangedFactTemplateItem
    {
        public string Language { get; set; }

        public LanguageProficiency Proficiency { get; set; }
    }

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
