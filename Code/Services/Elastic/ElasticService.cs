using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Impworks.Utils.Linq;
using Impworks.Utils.Strings;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json.Linq;
using Page = Bonsai.Data.Models.Page;

namespace Bonsai.Code.Services.Elastic
{
    /// <summary>
    /// The low-level service for working with ElasticSearch.
    /// </summary>
    public class ElasticService
    {
        public ElasticService(ElasticClient client)
        {
            _client = client;
        }

        private readonly ElasticClient _client;

        private const int PAGE_SIZE = 20;
        private const int FRAGMENT_SIZE = 200;

        private const string PAGE_INDEX = "pages";
        private const string STOP_WORDS = "а,без,более,бы,был,была,были,было,быть,в,вам,вас,весь,во,вот,все,всего,всех,вы,где,да,даже,для,до,его,ее,если,есть,еще,же,за,здесь,и,из,или,им,их,к,как,ко,когда,кто,ли,либо,мне,может,мы,на,надо,наш,не,него,нее,нет,ни,них,но,ну,о,об,однако,он,она,они,оно,от,очень,по,под,при,с,со,так,также,такой,там,те,тем,то,того,тоже,той,только,том,ты,у,уже,хотя,чего,чей,чем,что,чтобы,чье,чья,эта,эти,это,я";

        #region Initialization

        /// <summary>
        /// Removes all cached data.
        /// </summary>
        public async Task ClearPreviousDataAsync()
        {
            var result = await _client.IndexExistsAsync(PAGE_INDEX);

            if(!result.IsValid)
                throw result.OriginalException;

            if(result.Exists)
                await _client.DeleteIndexAsync(PAGE_INDEX);
        }

        /// <summary>
        /// Creates all required indexes.
        /// </summary>
        public async Task EnsureIndexesCreatedAsync(AppDbContext db = null)
        {
            var check = await _client.IndexExistsAsync(PAGE_INDEX);
            if (check.Exists)
                return;

            var result = await _client.CreateIndexAsync(
                PAGE_INDEX,
                m => m.Mappings(mp =>
                    mp.Map<PageDocument>(mx =>
                        mx.Properties(p =>
                            p.Text(x =>
                                x.Name(f => f.Title)
                                 .Analyzer("index_ru")
                                 .SearchAnalyzer("search_ru")
                            )
                             .Text(x =>
                               x.Name(f => f.Aliases)
                                .Analyzer("index_ru")
                                .SearchAnalyzer("search_ru")
                             )
                            .Text(x =>
                                x.Name(f => f.Description)
                                 .Analyzer("index_ru")
                                 .SearchAnalyzer("search_ru")
                            )
                             .Scalar(x => x.PageType)
                        )
                    )
                )
                .Settings(s =>
                    s.Analysis(a =>
                        a.CharFilters(c =>
                            c.Mapping("filter_ru_e", z => z.Mappings("Ё=>Е", "ё=>е"))
                        )
                        .TokenFilters(t =>
                            t.Stop("stopwords_ru", st =>
                                st.StopWords(STOP_WORDS.Split(','))
                                  .IgnoreCase()
                            )
                            .WordDelimiter("delim_ru", d =>
                                d.GenerateWordParts(true)
                                 .GenerateNumberParts(true)
                                 .CatenateWords(true)
                                 .CatenateNumbers(false)
                                 .CatenateAll(true)
                                 .SplitOnCaseChange(true)
                                 .SplitOnNumerics(false)
                                 .PreserveOriginal(true)
                            )
                        )
                        .Analyzers(an =>
                            an.Custom("index_ru", ac =>
                                ac.CharFilters("html_strip", "filter_ru_e")
                                .Tokenizer("standard")
                                .Filters("stopwords_ru", "delim_ru", "stop", "lowercase", "russian_morphology", "english_morphology")
                            )
                            .Custom("search_ru", ac =>
                                ac.CharFilters("html_strip", "filter_ru_e")
                                .Tokenizer("standard")
                                .Filters("stopwords_ru", "delim_ru", "stop", "lowercase", "russian_morphology", "english_morphology")
                            )
                        )
                    )
                )
            );

            if(!result.IsValid)
                throw result.OriginalException;

            if (db != null)
                await ReindexAllPagesAsync(db);
        }

        /// <summary>
        /// Adds a page to the index.
        /// </summary>
        public async Task AddPageAsync(Page page)
        {
            var doc = new PageDocument
            {
                Id = page.Id,
                Key = page.Key,
                Title = page.Title,
                Aliases = GetPageAliases(page).JoinString(", "),
                PageType = (int)page.Type,
                Description = MarkdownService.Strip(page.Description),
            };

            await _client.IndexAsync(doc, i => i.Index(PAGE_INDEX));
        }

        /// <summary>
        /// Removes a page from the index.
        /// </summary>
        public async Task RemovePageAsync(Page page)
        {
            await _client.DeleteAsync(new DeleteRequest(PAGE_INDEX, TypeName.From<PageDocument>(), page.Id));
        }

        /// <summary>
        /// Searches for the pages matching the query.
        /// </summary>
        public async Task<IReadOnlyList<PageDocumentSearchResult>> SearchAsync(string query, int page = 0)
        {
            const string PRE_TAG = "<b>";
            const string POST_TAG = "</b>";

            PageDocumentSearchResult Map(IHit<PageDocument> hit)
            {
                string GetHitValue(string fieldName, string fallback)
                {
                    if(hit.Highlights.TryGetValue(fieldName.ToLower(), out var hitList))
                    {
                        var hitValue = hitList.Highlights.First();
                        var rawHitValue = hitValue.Replace(PRE_TAG, "").Replace(POST_TAG, "");
                        var startEllipsis = !rawHitValue.StartsWithPart(fallback, 10);
                        var endEllipsis = !rawHitValue.EndsWithPart(fallback, 10);
                        if(!startEllipsis && !endEllipsis)
                            return hitValue;

                        return string.Concat(
                            startEllipsis ? "..." : "",
                            hitValue,
                            endEllipsis ? "..." : ""
                        );
                    }

                    if (fallback.Length > FRAGMENT_SIZE)
                        return fallback.Substring(0, FRAGMENT_SIZE) + "...";

                    return fallback;
                }

                return new PageDocumentSearchResult
                {
                    Id = hit.Source.Id,
                    Key = hit.Source.Key,

                    HighlightedTitle = GetHitValue(nameof(PageDocument.Title), hit.Source.Title),
                    HighlightedDescription = GetHitValue(nameof(PageDocument.Description), hit.Source.Description),
                };
            }

            var result = await _client.SearchAsync<PageDocument>(
                s => s.Index(PAGE_INDEX)
                      .Query(q =>
                          q.MatchPhrasePrefix(f => f.Field(x => x.Title).Query(query).Boost(100))
                          || q.MultiMatch(
                                f => f.Fields(x =>
                                        x.Fields(
                                            y => y.Title,
                                            y => y.Description,
                                            y => y.Aliases
                                        )
                                    )
                                    .Query(query)
                                    .Fuzziness(Fuzziness.EditDistance(1))
                          )
                      )
                      .Skip(PAGE_SIZE * page)
                      .Take(PAGE_SIZE)
                      .Highlight(
                          h => h.FragmentSize(FRAGMENT_SIZE)
                                .PreTags(PRE_TAG)
                                .PostTags(POST_TAG)
                                .BoundaryScanner(BoundaryScanner.Sentence)
                                .BoundaryScannerLocale("ru-RU")
                                .Fields(
                                    x => x.Field(f => f.Title),
                                    x => x.Field(f => f.Description)
                                )
                      )
            );

            return result.Hits.Select(Map).ToList();
        }

        /// <summary>
        /// Returns the probable matches for the search autocomplete.
        /// </summary>
        public async Task<IReadOnlyList<PageDocumentSearchResult>> SearchAutocompleteAsync(string query, IReadOnlyList<PageType> pageTypes = null, int? maxCount = null)
        {
            PageDocumentSearchResult Map(PageDocument doc)
            {
                return new PageDocumentSearchResult
                {
                    Id = doc.Id,
                    Key = doc.Key,
                    HighlightedTitle = doc.Title,
                    PageType = (PageType)doc.PageType
                };
            }

            pageTypes = pageTypes ?? EnumHelper.GetEnumValues<PageType>();

            var result = await _client.SearchAsync<PageDocument>(
                s => s.Index(PAGE_INDEX)
                      .Query(q =>
                                 q.Terms(f => f.Field(x => x.PageType).Terms(pageTypes))
                                 && (
                                     q.MatchPhrasePrefix(f => f.Field(x => x.Title).Query(query).Boost(100))
                                     || q.Match(f => f.Field(x => x.Aliases).Query(query).Fuzziness(Fuzziness.EditDistance(1)))
                                 )
                      )
                      .Take(maxCount ?? 5)
            );

            return result.Documents.Select(Map).ToList();
        }

        #endregion

        #region Reindex

        /// <summary>
        /// Updates all pages in the cache.
        /// </summary>
        private async Task ReindexAllPagesAsync(AppDbContext db)
        {
            var pages = await db.Pages
                                .Include(x => x.Aliases)
                                .AsNoTracking()
                                .ToListAsync();

            foreach (var page in pages)
                await AddPageAsync(page);
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Returns all aliases known for a page (including previous names).
        /// </summary>
        private IEnumerable<string> GetPageAliases(Page page)
        {
            var aliases = page.Aliases.Select(x => x.Title).ToList();

            try
            {
                if (page.Type == PageType.Person && !string.IsNullOrEmpty(page.Facts))
                {
                    var json = JObject.Parse(page.Facts);
                    var names = json["Main.Name"]?["Values"];

                    if (names != null)
                    {
                        foreach (var name in names)
                        {
                            var nameStr = name["LastName"] + " " + name["FirstName"] + " " + name["MiddleName"];
                            if(!string.IsNullOrWhiteSpace(nameStr))
                                aliases.Add(nameStr.Trim());
                        }
                    }
                }
            }
            catch
            {
                // skip
            }

            return aliases.Distinct();
        }

        #endregion
    }
}

