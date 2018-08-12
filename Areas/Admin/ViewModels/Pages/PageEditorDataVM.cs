using System.Collections.Generic;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Data.Models;
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
        public IReadOnlyDictionary<PageType, FactDefinitionGroup[]> FactGroups { get; set; }

        /// <summary>
        /// List of editor template files.
        /// </summary>
        public IReadOnlyList<string> EditorTemplates { get; set; }
    }
}
