using Bonsai.Code.Tools;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// A template item that has a duration period.
    /// </summary>
    public class RangedFactItem
    {
        /// <summary>
        /// Range of the item's actuality.
        /// </summary>
        public FuzzyRange Range { get; set; }
    }
}
