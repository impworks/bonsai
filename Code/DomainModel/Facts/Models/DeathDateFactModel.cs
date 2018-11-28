using Bonsai.Code.Utils.Date;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// Special logic for displaying a death date.
    /// </summary>
    public class DeathDateFactModel: FactModelBase
    {
        /// <summary>
        /// Arbitrary date value (optional).
        /// </summary>
        public FuzzyDate? Value { get; set; }

        /// <summary>
        /// Flag indicating that the person is dead, but the exact date is unknown.
        /// </summary>
        public bool IsUnknown { get; set; }

        /// <summary>
        /// Inner check.
        /// </summary>
        public override bool IsValid => Value.HasValue != IsUnknown;
    }
}
