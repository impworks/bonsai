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
        /// Returns the appropriate 
        /// </summary>
        public override string ShortTitle => Values.Length == 1 ? Definition.ShortTitleSingle : Definition.ShortTitleMultiple;
    }
}
