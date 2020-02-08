namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// A template for gender specification.
    /// </summary>
    public class GenderFactModel: FactModelBase
    {
        /// <summary>
        /// Flag indicating the gender.
        /// </summary>
        public bool IsMale { get; set; }
    }
}
