using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Type of an entity described by the page.
    /// </summary>
    public enum PageType
    {
        [Description("Человек")]
        Person = 0,

        [Description("Питомец")]
        Pet = 1,

        [Description("Событие")]
        Event = 2,

        [Description("Место")]
        Location = 3,

        [Description("Прочее")]
        Other = 4
    }
}
