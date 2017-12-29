using Bonsai.Code.DomainModel.Facts.Templates;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// Blueprint of a fact's template and editor.
    /// </summary>
    public class FactDefinition<TTemplate> : IFactDefinition
        where TTemplate: IFactTemplate
    {
        public FactDefinition(string id, string title)
        {
            Id = id;
            Title = title;
        }

        /// <summary>
        /// Unique ID for referencing the fact.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Readable title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Returns the path for display template.
        /// </summary>
        public string ViewTemplatePath => GetTemplatePath("~/Areas/Front/Views/Page/Facts/");

        /// <summary>
        /// Returns the path for editor template.
        /// </summary>
        public string EditTemplatePath => GetTemplatePath("~/Areas/Admin/Views/Editor/Facts/");

        /// <summary>
        /// Returns the path for current template.
        /// </summary>
        private string GetTemplatePath(string prefixPath)
        {
            var itemName = typeof(TTemplate).Name;
            return prefixPath + itemName + ".cshtml";
        }
    }
}
