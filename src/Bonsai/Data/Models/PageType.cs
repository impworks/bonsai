using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Type of an entity described by the page.
    /// </summary>
    public enum PageType
    {
        [Description("Человек")]
        Person,

        [Description("Питомец")]
        Pet,

        [Description("Событие")]
        Event,

        [Description("Место")]
        Location,

        [Description("Прочее")]
        Other
    }
}
