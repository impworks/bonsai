using Bonsai.Code.Tools;

namespace Bonsai.Code.DomainModel.Facts.Templates
{
    /// <summary>
    /// A template item that has a duration period.
    /// </summary>
    public class RangedFactTemplateItem
    {
        /// <summary>
        /// Start of the duration.
        /// </summary>
        public FuzzyDate RangeStart { get; set; }

        /// <summary>
        /// End of the duration.
        /// </summary>
        public FuzzyDate RangeEnd { get; set; }
    }
}
