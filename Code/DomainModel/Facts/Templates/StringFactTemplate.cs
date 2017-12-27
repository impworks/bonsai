namespace Bonsai.Code.DomainModel.Facts.Templates
{
    /// <summary>
    /// The template for specifying arbitrary string-based facts.
    /// </summary>
    public class StringFactTemplate: IFactTemplate
    {
        public string Value { get; set; }
    }
}
