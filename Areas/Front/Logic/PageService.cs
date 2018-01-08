using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Code.Services;
using Bonsai.Code.Tools;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// Page displayer service.
    /// </summary>
    public class PageService
    {
        public PageService(AppDbContext db, MarkdownService markdown)
        {
            _db = db;
            _markdown = markdown;
        }

        private readonly AppDbContext _db;
        private readonly MarkdownService _markdown;

        #region Public methods

        /// <summary>
        /// Returns the description VM for a page.
        /// </summary>
        public async Task<PageDescriptionVM> GetPageDescriptionAsync(string key)
        {
            var page = await _db.Pages
                                .AsNoTracking()
                                .Include(p => p.Relations)
                                .ThenInclude(r => r.Object)
                                .FirstOrDefaultAsync(x => x.Key == key)
                                .ConfigureAwait(false);

            if (page == null)
                throw new KeyNotFoundException();

            var descr = await _markdown.CompileAsync(page.Description)
                                       .ConfigureAwait(false);

            var factGroups = GetPersonalFacts(page).ToList();

            return Configure(page, new PageDescriptionVM
            {
                Description = descr,
                PhotoFact = GetFactModel<PhotoFactModel>(factGroups, "Common", "Photo"),
                NameFact = GetFactModel<NameFactModel>(factGroups, "Common", "Name"),
                PersonalFacts = factGroups.Where(x => x.Id != "Common").ToList(),

                // todo: relation facts (grouping, ordering)
                RelationFacts = new List<FactGroupVM>()
            });
        }

        /// <summary>
        /// Returns the list of media files.
        /// </summary>
        public async Task<PageMediaVM> GetPageMediaAsync(string key)
        {
            var page = await _db.Pages
                                .AsNoTracking()
                                .Include(p => p.MediaTags)
                                .ThenInclude(t => t.Media)
                                .FirstOrDefaultAsync(x => x.Key == key)
                                .ConfigureAwait(false);

            if (page == null)
                throw new KeyNotFoundException();

            var list = new List<MediaThumbnailVM>();
            foreach (var tag in page.MediaTags)
            {
                list.Add(new MediaThumbnailVM
                {
                    Type = tag.Media.Type,
                    MediaKey = tag.Media.Key,
                    ThumbnailUrl = Path.ChangeExtension(tag.Media.FilePath, ".thumb.jpg"),
                    Year = FuzzyDate.Parse(tag.Media.Date).ReadableYear
                });
            }

            return Configure(page, new PageMediaVM
            {
                Media = list
            });
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Sets additional properties on a page view model.
        /// </summary>
        private T Configure<T>(Page page, T vm)
            where T : PageTitleVM
        {
            vm.Title = page.Title;
            vm.Key = page.Key;

            return vm;
        }

        /// <summary>
        /// Returns the list of personal facts for a page.
        /// </summary>
        private IEnumerable<FactGroupVM> GetPersonalFacts(Page page)
        {
            if (string.IsNullOrEmpty(page.Facts))
                yield break;

            var pageFacts = JObject.Parse(page.Facts);

            foreach (var group in FactDefinitions.FactGroups[page.PageType])
            {
                var factsVms = new List<FactModelBase>();

                foreach (var fact in group.Facts)
                {
                    var key = group.Id + "." + fact.Id;
                    var factInfo = pageFacts[key];

                    if (factInfo == null)
                        continue;

                    var vm = (FactModelBase) JsonConvert.DeserializeObject(factInfo.ToString(), fact.Kind);
                    vm.Definition = fact;
                    factsVms.Add(vm);
                }

                if (factsVms.Count > 0)
                {
                    yield return new FactGroupVM
                    {
                        Title = group.Title,
                        Facts = factsVms
                    };
                }
            }
        }

        /// <summary>
        /// Finds a fact in the list of groups.
        /// </summary>
        /// <returns></returns>
        private T GetFactModel<T>(IEnumerable<FactGroupVM> groups, string groupId, string factId)
            where T : FactModelBase
        {
            return groups.FirstOrDefault(x => x.Id == groupId)?.Facts.FirstOrDefault(x => x.Definition.Id == factId) as T;
        }

        #endregion
    }
}
