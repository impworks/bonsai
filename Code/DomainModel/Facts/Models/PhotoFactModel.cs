using Bonsai.Code.Tools;

namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying the titular photo.
    /// </summary>
    public class PhotoFactModel : FactModelBase
    {
        /// <summary>
        /// URL of the titular photograph.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Date of the photograph's creation.
        /// </summary>
        public FuzzyDate? Date { get; set; }
    }
}
