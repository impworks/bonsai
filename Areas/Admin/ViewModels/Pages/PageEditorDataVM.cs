using System.Collections.Generic;
using Bonsai.Code.DomainModel.Facts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Additional information for the page editor.
    /// </summary>
    public class PageEditorDataVM
    {
        /// <summary>
        /// Flag indicating that a new page is being created.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Known page types.
        /// </summary>
        public IReadOnlyList<SelectListItem> PageTypes { get; set; }

        /// <summary>
        /// Known groups of facts.
        /// </summary>
        public IEnumerable<FactDefinitionGroup> FactGroups { get; set; }

        /// <summary>
        /// List of editor template files.
        /// </summary>
        public IReadOnlyList<string> EditorTemplates { get; set; }

        /// <summary>
        /// Currently active tab.
        /// </summary>
        public string Tab { get; set; }

        /// <summary>
        /// List of comma-separated fields that contain errors.
        /// </summary>
        public string ErrorFields { get; set; }
    }
}
