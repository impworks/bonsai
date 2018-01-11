using System.ComponentModel;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying posessed skills.
    /// </summary>
    public class SkillFactModel : FactModelBase
    {
        /// <summary>
        /// List of known skills.
        /// </summary>
        public SkillFactItem[] Values { get; set; }
    }

    /// <summary>
    /// Information about a single skill.
    /// </summary>
    public class SkillFactItem : DurationFactItem
    {
        /// <summary>
        /// Name of the skill.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Proficiency of the skill.
        /// </summary>
        public SkillProficiency? Proficiency { get; set; }
    }

    public enum SkillProficiency
    {
        [Description("Новичок")]
        Beginner,

        [Description("Любитель")]
        Intermediate,

        [Description("Эксперт")]
        Profound
    }
}
