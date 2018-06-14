using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Relations;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Services;
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
    public class PagePresenterService
    {
        public PagePresenterService(AppDbContext db, MarkdownService markdown, RelationsPresenterService relations)
        {
            _db = db;
            _markdown = markdown;
            _relations = relations;
        }

        private readonly AppDbContext _db;
        private readonly MarkdownService _markdown;
        private readonly RelationsPresenterService _relations;

        #region Public methods

        /// <summary>
        /// Returns the description VM for a page.
        /// </summary>
        public async Task<PageDescriptionVM> GetPageDescriptionAsync(string key)
        {
            var page = await _db.Pages
                                .Include(x => x.MainPhoto)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Key == key)
                                .ConfigureAwait(false);

            if (page == null)
                throw new KeyNotFoundException();

            var descr = await _markdown.CompileAsync(page.Description).ConfigureAwait(false);
            return await ConfigureAsync(page, new PageDescriptionVM { Description = descr }).ConfigureAwait(false);
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

            var media = page.MediaTags.Select(x => MediaPresenterService.GetMediaThumbnail(x.Media, MediaSize.Small));
            return await ConfigureAsync(page, new PageMediaVM { Media = media }).ConfigureAwait(false);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Sets additional properties on a page view model.
        /// </summary>
        private async Task<T> ConfigureAsync<T>(Page page, T vm)
            where T : PageVMBase
        {
            vm.Title = page.Title;
            vm.Key = page.Key;
            vm.Type = page.Type;

            vm.InfoBlock = await GetInfoBlockAsync(page).ConfigureAwait(false);            

            return vm;
        }

        /// <summary>
        /// Returns the data for the page's main block.
        /// </summary>
        private async Task<InfoBlockVM> GetInfoBlockAsync(Page page)
        {
            var factGroups = GetPersonalFacts(page).ToList();
            var relations = await _relations.GetRelationsForPage(page.Id).ConfigureAwait(false);

            return new InfoBlockVM
            {
                Photo = MediaPresenterService.GetMediaThumbnail(page.MainPhoto, MediaSize.Medium),
                Facts = factGroups,
                RelationGroups = relations,
            };
        }

        /// <summary>
        /// Returns the list of personal facts for a page.
        /// </summary>
        private IEnumerable<FactGroupVM> GetPersonalFacts(Page page)
        {
            if (string.IsNullOrEmpty(page.Facts))
                yield break;

            var pageFacts = JObject.Parse(page.Facts);

            foreach (var group in FactDefinitions.Groups[page.Type])
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

                    if(!vm.IsHidden)
                        factsVms.Add(vm);
                }

                if (factsVms.Count > 0)
                {
                    yield return new FactGroupVM
                    {
                        Definition = group,
                        Facts = factsVms
                    };
                }
            }
        }

        #endregion
    }
}
