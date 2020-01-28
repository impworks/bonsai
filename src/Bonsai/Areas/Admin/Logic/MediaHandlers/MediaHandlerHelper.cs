using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;

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
        private static ImageCodecInfo JpegEncoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);

        /// <summary>
        /// Parameters for JPEG  encoder.
        /// </summary>
        private static EncoderParameters JpegEncoderArgs = new EncoderParameters { Param = new[] { new EncoderParameter(Encoder.Quality, 90L) } };

        #endregion

        #region Public methods

        /// <summary>
        /// Creates thumbnails.
        /// </summary>
        public static void CreateThumbnails(string path, Image frame)
        {
            foreach (var size in Sizes)
            {
                var thumbPath = MediaPresenterService.GetSizedMediaPath(path, size.Key);
                using (var image = ResizeToFit(frame, size.Value))
                    image.Save(thumbPath, JpegEncoder, JpegEncoderArgs);
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
            var srcRect = new Rectangle(0, 0, source.Width, source.Height);
            var destRect = new Rectangle(0, 0, propSize.Width, propSize.Height);

            var bmp = new Bitmap(destRect.Width, destRect.Height, PixelFormat.Format32bppArgb);
            using(var gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.Transparent);
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfx.DrawImage(
                    source,
                    destRect,
                    srcRect,
                    GraphicsUnit.Pixel
                );
            }

            return bmp;
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
