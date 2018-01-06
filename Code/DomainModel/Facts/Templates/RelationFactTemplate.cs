using Bonsai.Code.Tools;

namespace Bonsai.Code.DomainModel.Facts.Templates
{
    /// <summary>
    /// The template for specifying a relation between two entities.
    /// </summary>
    public class RelationFactTemplate: IFactTemplate
    {
        public string Title { get; set; }

        public string Key { get; set; }

        public FuzzyRange? Range { get; set; }
    }
}
