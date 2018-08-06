using Bonsai.Code.Utils.Date;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying a date.
    /// </summary>
    public class DateFactModel: FactModelBase
    {
        /// <summary>
        /// Arbitrary date value.
        /// </summary>
        public FuzzyDate Value { get; set; }
    }
}
