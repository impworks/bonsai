using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels;
using Bonsai.Code.Services;
using Bonsai.Code.Tools;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// Media displayer service.
    /// </summary>
    public class MediaService
    {
        public MediaService(AppDbContext db, MarkdownService markdown)
        {
            _db = db;
            _markdown = markdown;
        }

        private readonly AppDbContext _db;
        private readonly MarkdownService _markdown;

        /// <summary>
        /// Returns the details for a media.
        /// </summary>
        public async Task<MediaVM> GetMediaAsync(string key)
        {
            var media = await _db.Media
                                 .Include(x => x.Tags)
                                 .ThenInclude(x => x.Object)
                                 .FirstOrDefaultAsync(x => x.Key == key)
                                 .ConfigureAwait(false);

            if (media == null)
                throw new KeyNotFoundException();

            var vm = new MediaVM
            {
                Type = media.Type,
                Description = _markdown.Compile(media.Description),
                Date = FuzzyDate.TryParse(media.Date),
                Tags = GetMediaTagsVMs(media.Tags).ToList(),
                Event = GetPageTitle(media.Tags.FirstOrDefault(x => x.Type == MediaTagType.Event)),
                Location = GetPageTitle(media.Tags.FirstOrDefault(x => x.Type == MediaTagType.Location))
            };

            if (media.Type == MediaType.Photo || media.Type == MediaType.Document)
            {
                vm.FullPath = media.FilePath;
                vm.MediaPath = Path.ChangeExtension(media.FilePath, ".sm.jpg");
            }
            else
            {
                vm.MediaPath = media.FilePath;
                vm.FullPath = null;
            }

            return vm;
        }

        /// <summary>
        /// Maps a media tag to a page description.
        /// </summary>
        private PageTitleVM GetPageTitle(MediaTag tag)
        {
            if(tag == null)
                return null;

            return new PageTitleVM
            {
                Title = tag.Object.Title,
                Key = tag.Object.Key
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

                var coords = str.Split(',')
                                .Select(x => float.Parse(x, CultureInfo.InvariantCulture))
                                .ToList();

                return RectangleF.FromLTRB(
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
                    Page = GetPageTitle(tag),
                    Rect = ParseRectangle(tag.Coordinates)
                };
            }
        }
    }
}
