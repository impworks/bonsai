namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// A list of string values, defined by date.
    /// </summary>
    public class StringListFactModel: FactModelBase
    {
        /// <summary>
        /// List of skill values.
        /// </summary>
        public StringListFactItem[] Values { get; set; }
    }

    /// <summary>
    /// Current value & duration wrapper.
    /// </summary>
    public class StringListFactItem : DurationFactItem
    {
        /// <summary>
        /// Current value.
        /// </summary>
        public string Value { get; set; }
    }
}
