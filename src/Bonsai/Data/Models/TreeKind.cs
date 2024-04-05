using System;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Tree kinds which can be configured to auto-calculate.
    /// </summary>
    [Flags]
    public enum TreeKind
    {
        /// <summary>
        /// The whole connected graph of nodes.
        /// </summary>
        FullTree = 1,

        /// <summary>
        /// Two levels up and two levels down around a person.
        /// </summary>
        CloseFamily = 2,

        /// <summary>
        /// Blood ancestors of a person.
        /// </summary>
        Ancestors = 4,

        /// <summary>
        /// Direct descendants of a person, including spouses.
        /// </summary>
        Descendants = 8
    }
}
