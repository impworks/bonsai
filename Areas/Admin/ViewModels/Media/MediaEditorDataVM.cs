using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// Strongly typed structure with additional properties of the media editor.
    /// </summary>
    public class MediaEditorDataVM
    {
        public IEnumerable<SelectListItem> LocationItem { get; set; }
        public IEnumerable<SelectListItem> EventItem { get; set; }

        public IEnumerable<MediaTagVM> DepictedEntities { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}
