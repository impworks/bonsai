using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Relations;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Services;
using Bonsai.Code.Utils;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// Page displayer service.
    /// </summary>
    public class PagePresenterService
    {
        public PagePresenterService(AppDbContext db, MarkdownService markdown, RelationsPresenterService relations, TreePresenterService tree)
        {
            _db = db;
            _markdown = markdown;
            _relations = relations;
            _tree = tree;
        }

        private readonly AppDbContext _db;
        private readonly MarkdownService _markdown;
        private readonly RelationsPresenterService _relations;
        private readonly TreePresenterService _tree;

        #region Public methods

        /// <summary>
        /// Returns the description VM for a page.
        /// </summary>
        public async Task<PageDescriptionVM> GetPageDescriptionAsync(string key)
        {
            var page = await FindPageAsync(key);
            return await GetPageDescriptionAsync(page);
        }

        /// <summary>
        /// Returns the description for a constructed page.
        /// </summary>
        public async Task<PageDescriptionVM> GetPageDescriptionAsync(Page page)
        {
            var descr = await _markdown.CompileAsync(page.Description);
            return Configure(page, new PageDescriptionVM { Description = descr });
        }

        /// <summary>
        /// Returns the list of media files.
        /// </summary>
        public async Task<PageMediaVM> GetPageMediaAsync(string key)
        {
            var page = await FindPageAsync(key, q => q.Include(p => p.MediaTags)
                                                      .ThenInclude(t => t.Media));

            var media = page.MediaTags
                            .Where(x => x.Media.IsDeleted == false)
                            .Select(x => MediaPresenterService.GetMediaThumbnail(x.Media, MediaSize.Small))
                            .ToList();

            return Configure(page, new PageMediaVM { Media = media });
        }

        /// <summary>
        /// Returns the contents of the family tree.
        /// </summary>
        public async Task<PageTreeVM> GetPageTreeAsync(string key)
        {
            var page = await FindPageAsync(key);

            if(page.Type != PageType.Person)
                throw new KeyNotFoundException();

            var tree = await _tree.GetTreeAsync(page.Id);
            return Configure(page, new PageTreeVM {Tree = tree});
        }

        /// <summary>
        /// Returns the data for the page's side block.
        /// </summary>
        public async Task<InfoBlockVM> GetPageInfoBlockAsync(string key)
        {
            var keyLower = key?.ToLowerInvariant();
            var page = await _db.Pages
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Aliases.Any(y => y.Key == keyLower) && x.IsDeleted == false);

            if (page == null)
                throw new KeyNotFoundException();

            return await GetPageInfoBlockAsync(page);
        }

        /// <summary>
        /// Returns the info block for a constructed page.
        /// </summary>
        public async Task<InfoBlockVM> GetPageInfoBlockAsync(Page page)
        {
            var factGroups = GetPersonalFacts(page).ToList();
            var relations = await _relations.GetRelationsForPage(page.Id);

            return new InfoBlockVM
            {
                Photo = MediaPresenterService.GetMediaThumbnail(page.MainPhoto, MediaSize.Medium),
                Facts = factGroups,
                RelationGroups = relations,
            };
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Sets additional properties on a page view model.
        /// </summary>
        private T Configure<T>(Page page, T vm) where T : PageTitleVM
        {
            vm.Id = page.Id;
            vm.Title = page.Title;
            vm.Key = page.Key;
            vm.Type = page.Type;

            return vm;
        }

        /// <summary>
        /// Returns the page by its key.
        /// </summary>
        private async Task<Page> FindPageAsync(string key, Func<IQueryable<Page>, IQueryable<Page>> config = null)
        {
            var query = _db.Pages
                           .AsNoTracking()
                           .Include(x => x.MainPhoto) as IQueryable<Page>;

            if (config != null)
                query = config(query);

            var keyLower = key?.ToLowerInvariant();
            var page = await query.FirstOrDefaultAsync(x => x.Aliases.Any(y => y.Key == keyLower) && x.IsDeleted == false);

            if (page == null)
                throw new KeyNotFoundException();

            if (page.Key != key)
                throw new RedirectRequiredException(page.Key);

            return page;
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

                foreach (var fact in group.Defs)
                {
                    var key = group.Id + "." + fact.Id;
                    var factInfo = pageFacts[key];

                    var vm = Deserialize(factInfo, fact.Kind);
                    if (vm == null)
                        continue;

                    vm.Definition = fact;

                    if (!vm.IsHidden)
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

        /// <summary>
        /// Attempts to deserialize the fact.
        /// Returns null on error.
        /// </summary>
        private FactModelBase Deserialize(JToken json, Type kind)
        {
            if (json == null)
                return null;

            try
            {
                return (FactModelBase) json.ToObject(kind);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
