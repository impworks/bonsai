namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// The template for specifying a list of values.
    /// </summary>
    public class FactListModelBase<T>: FactModelBase
    {
        /// <summary>
        /// List of values.
        /// </summary>
        public T[] Values { get; set; }

        /// <summary>
        /// Flag indicating that this fact does not contain any data.
        /// </summary>
        public override bool IsHidden => Values == null || Values.Length == 0;

        /// <summary>
        /// Returns the appropriate short title depending on the number of values.
        /// </summary>
        public override string ShortTitle => Values.Length == 1 ? Definition.ShortTitleSingle : Definition.ShortTitleMultiple;
    }
}
