using System.IO;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Hosting;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper class for working with server's state.
    /// </summary>
    public static class IWebHostEnvironmentExtensions
    {
        /// <summary>
        /// Returns the path for the media's file on the disk.
        /// </summary>
        public static string GetMediaPath(this IWebHostEnvironment env, Media media, MediaSize size = MediaSize.Original)
        {
            var fileName = Path.GetFileName(media.FilePath);
            var localPath = Path.Combine(env.WebRootPath, "media", fileName);
            return MediaPresenterService.GetSizedMediaPath(localPath, size);
        }
    }
}
