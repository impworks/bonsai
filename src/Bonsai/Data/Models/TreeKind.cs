using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Tree kinds which can be configured to auto-calculate.
    /// </summary>
    public enum TreeKind
    {
        /// <summary>
        /// The whole connected graph of nodes.
        /// </summary>
        [Description("Полное")]
        FullTree = 1,

        /// <summary>
        /// Two levels up and two levels down around a person.
        /// </summary>
        [Description("Семья")]
        CloseFamily = 2,

        /// <summary>
        /// Blood ancestors of a person.
        /// </summary>
        [Description("Предки")]
        Ancestors = 4,

        /// <summary>
        /// Direct descendants of a person, including spouses.
        /// </summary>
        [Description("Потомки")]
        Descendants = 8
    }
}
