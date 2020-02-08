namespace Bonsai.Code.DomainModel.Facts.Models
{
    /// <summary>
    /// Base class for all fact templates.
    /// </summary>
    public abstract class FactModelBase
    {
        /// <summary>
        /// Definition of the current fact.
        /// </summary>
        public IFactDefinition Definition { get; set; }

        /// <summary>
        /// Flag indicating that the current fact does not contain valuable information
        /// and must be omitted from the side bar.
        /// </summary>
        public virtual bool IsHidden => false;

        /// <summary>
        /// Title.
        /// </summary>
        public virtual string ShortTitle => Definition.ShortTitleSingle;

        /// <summary>
        /// Flag's inner consistency validation.
        /// </summary>
        public virtual bool IsValid => true;
    }
}
