namespace Bonsai.Code.DomainModel.Facts.Templates
{
    /// <summary>
    /// The template definition for a person's name.
    /// </summary>
    public class NameFactTemplate: IFactTemplate
    {
        public NameFactTemplateItem[] Values { get; set; }
    }

    /// <summary>
    /// A single recorded name with date ranges.
    /// </summary>
    public class NameFactTemplateItem: RangedFactTemplateItem
    {
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }
    }
}
