using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Services;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// Media displayer service.
    /// </summary>
    public class MediaPresenterService
    {
        public MediaPresenterService(AppDbContext db, MarkdownService markdown)
        {
            _db = db;
            _markdown = markdown;
        }

        private readonly AppDbContext _db;
        private readonly MarkdownService _markdown;

        #region Methods

        /// <summary>
        /// Returns the details for a media.
        /// </summary>
        public async Task<MediaVM> GetMediaAsync(string key)
        {
            var id = PageHelper.GetMediaId(key);
            var media = await _db.Media
                                 .Include(x => x.Tags)
                                 .ThenInclude(x => x.Object)
                                 .Where(x => x.IsDeleted == false)
                                 .FirstOrDefaultAsync(x => x.Id == id);

            if (media == null)
                throw new KeyNotFoundException();

            var descr = await _markdown.CompileAsync(media.Description);

            return new MediaVM
            {
                Id = media.Id,
                Type = media.Type,
                IsProcessed = media.IsProcessed,
                Title = media.Title,
                Description = descr,
                Date = FuzzyDate.TryParse(media.Date),
                Tags = GetMediaTagsVMs(media.Tags).ToList(),
                Event = GetPageTitle(media.Tags.FirstOrDefault(x => x.Type == MediaTagType.Event)),
                Location = GetPageTitle(media.Tags.FirstOrDefault(x => x.Type == MediaTagType.Location)),
                OriginalPath = media.FilePath,
                PreviewPath = GetSizedMediaPath(media.FilePath, MediaSize.Large)
            };
        }

        /// <summary>
        /// Returns the last N uploaded images.
        /// </summary>
        public async Task<IReadOnlyList<MediaThumbnailVM>> GetLastUploadedMediaAsync(int count)
        {
            return await _db.Media
                            .Where(x => !x.IsDeleted)
                            .OrderByDescending(x => x.UploadDate)
                            .Take(count)
                            .Select(x => new MediaThumbnailVM
                            {
                                Key = x.Key,
                                Type = x.Type,
                                ThumbnailUrl = GetSizedMediaPath(x.FilePath, MediaSize.Small),
                                Date = FuzzyDate.TryParse(x.Date),
                            })
                            .ToListAsync();
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Maps a media tag to a page description.
        /// </summary>
        private PageTitleVM GetPageTitle(MediaTag tag)
        {
            if(tag == null)
                return null;

            return new PageTitleVM
            {
                Id = tag.ObjectId,
                Title = tag.Object?.Title ?? tag.ObjectTitle,
                Key = tag.Object?.Key,
                Type = tag.Object?.Type ?? PageType.Other
            };
        }

        /// <summary>
        /// Converts the media tag data objects to corresponding VMs.
        /// </summary>
        private IEnumerable<MediaTagVM> GetMediaTagsVMs(IEnumerable<MediaTag> tags)
        {
            RectangleF? ParseRectangle(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return null;

                var coords = str.Split(';')
                                .Select(x => float.Parse(x, CultureInfo.InvariantCulture))
                                .ToList();

                return new RectangleF(
                    coords[0],
                    coords[1],
                    coords[2],
                    coords[3]
                );
            }

            foreach (var tag in tags)
            {
                if (tag.Type != MediaTagType.DepictedEntity)
                    continue;

                yield return new MediaTagVM
                {
                    TagId = tag.Id,
                    Page = GetPageTitle(tag),
                    Rect = ParseRectangle(tag.Coordinates)
                };
            }
        }

        #endregion

        #region Static helpers

        /// <summary>
        /// Gets the file path for a media frame of specified size.
        /// </summary>
        public static string GetSizedMediaPath(string fullPath, MediaSize size)
        {
            if (string.IsNullOrEmpty(fullPath))
                return fullPath;

            if (size == MediaSize.Original)
                return fullPath;

            if (size == MediaSize.Large)
                return Path.ChangeExtension(fullPath, ".lg.jpg");

            if (size == MediaSize.Medium)
                return Path.ChangeExtension(fullPath, ".md.jpg");

            if (size == MediaSize.Small)
                return Path.ChangeExtension(fullPath, ".sm.jpg");

            throw new ArgumentOutOfRangeException(nameof(size), "Unexpected media size!");
        }

        /// <summary>
        /// Returns the photo model.
        /// </summary>
        public static MediaThumbnailVM GetMediaThumbnail(Media media, MediaSize size = MediaSize.Small)
        {
            if (media == null)
                return null;

            return new MediaThumbnailVM
            {
                Type = media.Type,
                Key = media.Key,
                ThumbnailUrl = GetSizedMediaPath(media.FilePath, size),
                Date = FuzzyDate.TryParse(media.Date)
            };
        }

        #endregion
    }
}
