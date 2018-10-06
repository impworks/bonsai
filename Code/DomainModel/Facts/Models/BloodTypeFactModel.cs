using System.ComponentModel;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying blood type.
    /// </summary>
    public class BloodTypeFactModel: FactModelBase
    {
        /// <summary>
        /// Blood type.
        /// </summary>
        public BloodType Type { get; set; }

        /// <summary>
        /// Rhesus factor (if known).
        /// </summary>
        public bool? RhesusFactor { get; set; }
    }

    public enum BloodType
    {
        [Description("0 (I)")]
        O,

        [Description("A (II)")]
        A,

        [Description("B (III)")]
        B,

        [Description("AB (IV)")]
        AB
    }
}
