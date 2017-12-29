using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Templates;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// Page management service.
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

        /// <summary>
        /// List of specially handled facts which should not be displayed on the facts page.
        /// </summary>
        private static readonly string[] ExcludedFacts = {"common.photo"};

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

            // todo: main block
            return Configure(page, new PageDescriptionVM
            {
                Description = _markdown.Compile(page.Description)
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
                    // todo: year
                });
            }

            return Configure(page, new PageMediaVM
            {
                Media = list
            });
        }

        /// <summary>
        /// Returns the list of fact groups.
        /// </summary>
        public async Task<PageFactsVM> GetPageFactsAsync(string key)
        {
            var page = await _db.Pages
                                .AsNoTracking()
                                .Include(p => p.Relations)
                                .ThenInclude(t => t.Object)
                                .FirstOrDefaultAsync(x => x.Key == key)
                                .ConfigureAwait(false);

            if (page == null)
                throw new KeyNotFoundException();

            var groups = GetPersonalFacts(page)
                .Concat(GetRelationFacts(page))
                .ToList();

            return Configure(page, new PageFactsVM
            {
                FactGroups = groups
            });
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Sets additional properties on a page view model.
        /// </summary>
        private T Configure<T>(Page page, T vm)
            where T : PageVMBase
        {
            vm.Title = page.Title;
            vm.Key = page.Key;

            return vm;
        }

        /// <summary>
        /// Returns the list of fact groups based converted from relations.
        /// </summary>
        private IEnumerable<FactGroupVM> GetRelationFacts(Page page)
        {
            if (page.Relations.Count == 0)
                yield break;

            RelationFactTemplate Converter(Relation r) => new RelationFactTemplate
            {
                Name = r.ObjectTitle,
                ObjectKey = r.Object.Key
            };

            var templatePath = new FactDefinition<RelationFactTemplate>(null, null).ViewTemplatePath;
            var rels = page.Relations.GroupBy(x => x.Type).ToList();

            foreach (var relGroup in RelationGroups.List)
            {
                var facts = rels.Where(x => relGroup.Types.Contains(x.Key))
                                .Select(x => x.OrderBy(y => y.ObjectTitle))
                                .SelectMany(x => x)
                                .Select(x => new FactVM {Data = Converter(x), TemplatePath = templatePath, Title = x.Title})
                                .ToList();

                if (facts.Count > 0)
                {
                    yield return new FactGroupVM
                    {
                        Title = relGroup.Title,
                        Facts = facts
                    };
                }
            }
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
                var factsVms = new List<FactVM>();

                foreach (var fact in group.Facts)
                {
                    var key = group.Id + "." + fact.Id;
                    var factInfo = pageFacts[key];

                    if (factInfo == null || ExcludedFacts.Contains(key))
                        continue;

                    factsVms.Add(new FactVM
                    {
                        Title = fact.Title,
                        TemplatePath = fact.ViewTemplatePath,
                        Data = factInfo
                    });
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

        #endregion
    }
}
