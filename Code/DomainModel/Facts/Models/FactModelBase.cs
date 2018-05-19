using Newtonsoft.Json;

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
        [JsonIgnore]
        public IFactDefinition Definition { get; set; }

        /// <summary>
        /// Returns the path for display template.
        /// </summary>
        public string ViewTemplatePath => GetTemplatePath("~/Areas/Front/Views/Page/InfoBlock/");

        /// <summary>
        /// Returns the path for editor template.
        /// </summary>
        public string EditTemplatePath => GetTemplatePath("~/Areas/Admin/Views/Editor/Facts/");

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
        /// Returns the path for current template.
        /// </summary>
        private string GetTemplatePath(string prefixPath)
        {
            var itemName = GetType().Name;
            return prefixPath + itemName.Replace("Model", "Template") + ".cshtml";
        }
    }
}
