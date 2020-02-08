namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template definition for a person's name.
    /// </summary>
    public class HumanNameFactModel: FactListModelBase<HumanNameFactItem>
    {
        /// <summary>
        /// Hides the names list from the side bar unless there are at least two.
        /// The name is always displayed at the page's title.
        /// </summary>
        public override bool IsHidden => Values.Length < 2;
    }

    /// <summary>
    /// A single recorded name with date ranges.
    /// </summary>
    public class HumanNameFactItem: DurationFactItem
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
