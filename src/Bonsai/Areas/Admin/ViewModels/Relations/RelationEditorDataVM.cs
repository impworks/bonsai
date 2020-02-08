using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonsai.Areas.Admin.ViewModels.Relations
{
    /// <summary>
    /// Strongly typed structure for additional properties of the Relation editor.
    /// </summary>
    public class RelationEditorDataVM
    {
        public bool IsNew { get; set; }

        public RelationEditorPropertiesVM Properties { get; set; }

        public IEnumerable<SelectListItem> SourceItems { get; set; }
        public IEnumerable<SelectListItem> DestinationItem { get; set; }
        public IEnumerable<SelectListItem> EventItem { get; set; }
        public IEnumerable<SelectListItem> RelationTypes { get; set; }
    }
}
