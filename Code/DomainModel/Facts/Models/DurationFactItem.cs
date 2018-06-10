using Bonsai.Code.Utils.Date;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// A template item that has a duration period.
    /// </summary>
    public class DurationFactItem
    {
        /// <summary>
        /// Range of the item's actuality.
        /// </summary>
        public FuzzyRange? Duration { get; set; }
    }
}
