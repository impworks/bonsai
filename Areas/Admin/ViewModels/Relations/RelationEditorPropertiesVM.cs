using System.Collections.Generic;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Relations
{
    /// <summary>
    /// Extended properties for the relation editor.
    /// </summary>
    public class RelationEditorPropertiesVM
    {
        public string SourceName { get; set; }
        public string DestinationName { get; set; }

        public IReadOnlyList<PageType> SourceTypes { get; set; }
        public IReadOnlyList<PageType> DestinationTypes { get; set; }

        public bool ShowDuration { get; set; }
        public bool ShowEvent { get; set; }
    }
}
