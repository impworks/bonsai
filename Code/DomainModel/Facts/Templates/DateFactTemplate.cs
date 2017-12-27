using Bonsai.Code.Tools;

namespace Bonsai.Code.DomainModel.Facts.Templates
{
    /// <summary>
    /// The template for specifying a date.
    /// </summary>
    public class DateFactTemplate: IFactTemplate
    {
        public FuzzyDate Value { get; set; }
    }
}
