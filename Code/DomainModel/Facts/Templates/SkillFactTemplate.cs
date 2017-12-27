namespace Bonsai.Code.DomainModel.Facts.Templates
{
    /// <summary>
    /// The template for specifying posessed skills.
    /// </summary>
    public class SkillFactTemplate : IFactTemplate
    {
        public SkillFactTemplateItem[] Values { get; set; }
    }

    public class SkillFactTemplateItem : RangedFactTemplateItem
    {
        public string Language { get; set; }

        public SkillProficiency Proficiency { get; set; }
    }

    public enum SkillProficiency
    {
        Beginner,
        Intermediate,
        Profound
    }
}
