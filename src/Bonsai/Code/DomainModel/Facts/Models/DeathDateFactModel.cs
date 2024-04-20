using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data.Models;
using Bonsai.Localization;

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
        public override void Validate()
        {
            if (IsUnknown == Value.HasValue)
                throw new ValidationException(nameof(Page.Facts), Texts.Admin_Page_Facts_Validation_DeathDate);
        }
    }
}
