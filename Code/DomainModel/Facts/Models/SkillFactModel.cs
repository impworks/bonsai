using System.ComponentModel;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying posessed skills.
    /// </summary>
    public class SkillFactModel : FactListModelBase<SkillFactItem>
    {
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

    /// <summary>
    /// The proficiency in current skill or hobby.
    /// </summary>
    // ReSharper disable UnusedMember.Global
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
