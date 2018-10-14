using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Kinds of entity relationships.
    /// </summary>
    public enum RelationType
    {
        [Description("Родитель")]
        Parent,
        [Description("Ребенок")]
        Child,
        [Description("Супруг")]
        Spouse,

        [Description("Приемный родитель")]
        StepParent,
        [Description("Приемный ребенок")]
        StepChild,

        [Description("Друг")]
        Friend,
        [Description("Коллега")]
        Colleague,

        [Description("Хозяин")]
        Owner,
        [Description("Питомец")]
        Pet,

        [Description("Место")]
        Location,
        [Description("Житель")]
        LocationInhabitant,

        [Description("Событие")]
        Event,
        [Description("Участник")]
        EventVisitor,

        [Description("Прочее")]
        Other
    }
}
