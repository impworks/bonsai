namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// A date model with description.
    /// </summary>
    public class AnnotatedDateFactModel: DateFactModel
    {
        /// <summary>
        /// Description of the date.
        /// </summary>
        public string Title { get; set; }
    }
}
