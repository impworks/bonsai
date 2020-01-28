namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying a name of a pet, event or location.
    /// </summary>
    public class NameFactModel: FactModelBase
    {
        /// <summary>
        /// Name as a string.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Should not be displayed.
        /// </summary>
        public override bool IsHidden => true;
    }
}
