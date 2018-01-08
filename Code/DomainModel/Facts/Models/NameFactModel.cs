namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template definition for a person's name.
    /// </summary>
    public class NameFactModel: FactModelBase
    {
        /// <summary>
        /// The list of names.
        /// </summary>
        public NameFactItem[] Values { get; set; }
    }

    /// <summary>
    /// A single recorded name with date ranges.
    /// </summary>
    public class NameFactItem: RangedFactItem
    {
        /// <summary>
        /// First name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Middle name.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Last name.
        /// </summary>
        public string LastName { get; set; }
    }
}
