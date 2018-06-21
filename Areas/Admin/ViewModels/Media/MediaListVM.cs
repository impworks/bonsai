using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Dashboard;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// List of found media files.
    /// </summary>
    public class MediaListVM: ListResultVM<MediaListRequestVM, MediaThumbnailExtendedVM>
    {
    }
}
