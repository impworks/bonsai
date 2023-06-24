using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// Helper methods for creating thumbnails.
    /// </summary>
    public class MediaHandlerHelper
    {
        #region Constants

        /// <summary>
        /// Absolute sizes in pixels for the media files.
        /// </summary>
        private static Dictionary<MediaSize, Size> Sizes = new Dictionary<MediaSize, Size>
        {
            [MediaSize.Small] = new Size(200, 200),
            [MediaSize.Medium] = new Size(640, 480),
            [MediaSize.Large] = new Size(1280, 768),
        };

        /// <summary>
        /// Encoder for JPEG thumbnails.
        /// </summary>
        private static IImageEncoder JpegEncoder = new JpegEncoder { Quality = 90 };

        #endregion

        #region Public methods

        /// <summary>
        /// Creates thumbnails.
        /// </summary>
        public static async Task CreateThumbnailsAsync(string path, Image frame, CancellationToken token = default)
        {
            foreach (var size in Sizes)
            {
                var thumbPath = MediaPresenterService.GetSizedMediaPath(path, size.Key);
                using var image = await Task.Run(() => ResizeToFit(frame, size.Value), token);
                await image.SaveAsync(thumbPath, JpegEncoder, token);
            }
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Generates the thumbnail version of the specified image.
        /// </summary>
        private static Image ResizeToFit(Image source, Size maxSize)
        {
            var propSize = GetProportionalSize(source.Size, maxSize);
            return source.Clone(x =>
            {
                x.Resize(propSize, KnownResamplers.Lanczos3, true);
            });
        }

        /// <summary>
        /// Calculates the rectangle into which the image thumbnail will be inscribed.
        /// </summary>
        private static Size GetProportionalSize(Size size, Size maxSize)
        {
            // do not upscale small images
            if(size.Width < maxSize.Width && size.Height < maxSize.Height)
                return size;

            var xRatio = (double)maxSize.Width / size.Width;
            var yRatio = (double)maxSize.Height / size.Height;
            var ratio = Math.Min(yRatio, xRatio);
            return new Size((int)(size.Width * ratio), (int)(size.Height * ratio));
        }

        #endregion
    }
}