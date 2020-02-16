using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Impworks.Utils.Strings;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Newtonsoft.Json.Linq;
using StringHelper = Impworks.Utils.Strings.StringHelper;

namespace Bonsai.Code.Services.Search
{
    /// <summary>
    /// Fulltext search service.
    /// </summary>
    public class LuceneNetService : ISearchEngine, IAsyncDisposable
    {
        private readonly IndexWriter _writer;

        public LuceneNetService()
        {
            var luceneVersion = LuceneVersion.LUCENE_48;
            var indexConfig = new IndexWriterConfig(luceneVersion, new ClassicAnalyzer(luceneVersion));
            
            _writer = new IndexWriter(new RAMDirectory(), indexConfig);
        }

        #region ISearchEngine implementation

        /// <summary>
        /// Sets up the service (not needed in this implementation).
        /// </summary>
        /// <returns></returns>
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds a new page to the index.
        /// </summary>
        public Task AddPageAsync(Page page)
        {
            var luceneDoc = new LuceneDocument(page);
            _writer.AddDocument(luceneDoc.Fields);
            _writer.Commit();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes a page from the index.
        /// </summary>
        public Task RemovePageAsync(Page page)
        {
            var query = new TermQuery(new Term("Id", page.Id.ToString()));
            _writer.DeleteDocuments(query);
            _writer.Commit();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes all data from the index.
        /// </summary>
        public Task ClearDataAsync()
        {
            _writer.DeleteAll();
            _writer.Commit();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the search results when the search is actually executed.
        /// </summary>
        public Task<IReadOnlyList<PageDocumentSearchResult>> SearchAsync(string phrase, int page = 0)
        {
            const int PAGE_SIZE = 24;
            
            var (documents, query) = SearchIndex(phrase);
            var highlighter = CreateHighlighter(query);

            var searchResultsDocuments = documents.Skip(PAGE_SIZE * page).Take(PAGE_SIZE).ToList();
            
            var results = searchResultsDocuments.Select(doc =>
            {
                Enum.TryParse(doc.Get("PageType"), out PageType pt);
                return new PageDocumentSearchResult
                {
                    Id = Guid.Parse(doc.Get("Id")),
                    Key = doc.Get("Key"),
                    PageType = pt,
                    Title = doc.Get("Title"),
                    HighlightedTitle = Highlight(doc, "Title", highlighter),
                    HighlightedDescription = Highlight(doc, "Description", highlighter, true)
                };
            });

            return Task.FromResult(results.ToReadOnlyList());
        }

        /// <summary>
        /// Returns the suggestion list for the search field.
        /// </summary>
        public Task<IReadOnlyList<PageDocumentSearchResult>> SuggestAsync(string phrase, IReadOnlyList<PageType> pageTypes = null, int? maxCount = null)
        {
            var (documents, query) = SearchIndex(phrase, pageTypes, maxCount, true);
            var highlighter = CreateHighlighter(query);

            var results = documents.Select(doc =>
            {
                Enum.TryParse(doc.Get("PageType"), out PageType pt);
                return new PageDocumentSearchResult
                {
                    Id = Guid.Parse(doc.Get("Id")),
                    Key = doc.Get("Key"),
                    Title = doc.Get("Title"),
                    HighlightedTitle = Highlight(doc, "Title", highlighter),
                    PageType = pt
                };
            });

            return Task.FromResult(results.ToReadOnlyList());
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Partitions the search phrase to separate terms.
        /// </summary>
        private IEnumerable<string> SplitTerms(string phrase)
        {
            var lastI = 0;
            for (var i = 0; i < phrase.Length; i++)
            {
                if (char.IsLetter(phrase[i]))
                    continue;

                var substring = phrase.Substring(lastI, i - lastI);
                if (!string.IsNullOrWhiteSpace(substring))
                    yield return substring;

                lastI = i + 1;
            }

            yield return phrase.Substring(lastI);
        }
        
        /// <summary>
        /// Executes the search query.
        /// </summary>
        private (List<Document> documents, Query booleanQuery) SearchIndex(string phrase, IReadOnlyList<PageType> pageTypes = null, int? maxCount = null, bool suggest = false)
        {
            phrase = (phrase ?? "").ToLower();
            
            using var directoryReader = _writer.GetReader(true);
            var searcher = new IndexSearcher(directoryReader);
            searcher.Similarity = new LMJelinekMercerSimilarity(0.2f);

            var words = SplitTerms(phrase).ToList();

            var booleanQuery = new BooleanQuery { MinimumNumberShouldMatch = 1 };

            var fields = suggest
                ? new [] { "Title", "Aliases"}
                : new [] { "Title", "Aliases", "Description" };

            for (var i = 0; i < words.Count; i++)
            {
                var boostBase = (words.Count - i + 1.0f) * 3;
                foreach (var f in fields)
                {
                    var term = new Term(f, words[i]);
                    booleanQuery.Add(new FuzzyQuery(term, 2, 0) { Boost = boostBase }, Occur.SHOULD);
                    if (suggest)
                    {
                        booleanQuery.Add(new PrefixQuery(term) { Boost = boostBase * 3 }, Occur.SHOULD);
                    }
                    else
                    {
                        var phraseQuery = new PhraseQuery { Boost = boostBase * 3, Slop = 2 };
                        phraseQuery.Add(term);
                        booleanQuery.Add(phraseQuery, Occur.SHOULD);
                    }
                }
            }

            if (pageTypes != null)
            {
                var subquery = new BooleanQuery { MinimumNumberShouldMatch = 1 };
                foreach (var type in pageTypes)
                {
                    var typeValue = type.ToString().ToLower();
                    var termQuery = new TermQuery(new Term("PageType", typeValue));
                    subquery.Add(termQuery, Occur.SHOULD);
                }
                booleanQuery.Add(subquery, Occur.MUST);
                booleanQuery.MinimumNumberShouldMatch++;
            }

            var searchResults = searcher.Search(booleanQuery, maxCount ?? int.MaxValue);
            var documents = searchResults.ScoreDocs.Select(v => directoryReader.Document(v.Doc)).ToList();
            
            return (documents, booleanQuery);
        }

        /// <summary>
        /// Highlights the found value in source text.
        /// </summary>
        private string Highlight(Document doc, string field, Highlighter highlighter, bool wrap = false)
        {
            var value = doc.Get(field);
            if (string.IsNullOrEmpty(value))
                return "";

            using var stream = _writer.Analyzer.GetTokenStream(field, value);
            var highlightedValue = StringHelper.Coalesce(highlighter.GetBestFragment(stream, value), value);
            return wrap ? WrapHighlight(highlightedValue, value) : highlightedValue;

            string WrapHighlight(string highlighted, string actual)
            {
                var bare = highlighted.Replace("<b>", "").Replace("</b>", "");
                var startEllipsis = !actual.StartsWithPart(bare, 10);
                var endEllipsis = !actual.EndsWithPart(bare, 10);

                return (startEllipsis ? "..." : "") + highlighted + (endEllipsis ? "..." : "");
            }
        }

        /// <summary>
        /// Creates a highlighter for current query.
        /// </summary>
        private Highlighter CreateHighlighter(Query query)
        {
            var formatter = new SimpleHTMLFormatter("<b>", "</b>");
            var scorer = new QueryScorer(query);
            return new Highlighter(formatter, scorer) { TextFragmenter = new SimpleSpanFragmenter(scorer, 150) };
        }

        #endregion

        /// <summary>
        /// Releases the associated resources.
        /// </summary>
        public ValueTask DisposeAsync()
        {
            _writer.Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        #region LuceneDocument helper class

        /// <summary>
        /// Map between PageDocument and Lucene's internal representation.
        /// </summary>
        private class LuceneDocument
        {
            static LuceneDocument()
            {
                var indexedField = new FieldType {IsIndexed = true, IsStored = true, IsTokenized = true};
                var storedField = new FieldType {IsStored = true, IsIndexed = true};

                KnownFields = new Dictionary<string, Func<Page, Field>>
                {
                    { "Id", p => new Field("Id", p.Id.ToString(), storedField) },
                    { "Key", p => new Field("Key", p.Key, indexedField) },
                    { "Title", p => new Field("Title", p.Title, indexedField) { Boost = 500 } },
                    { "Aliases", p => new Field("Aliases", GetPageAliases(p).JoinString(", "), indexedField) { Boost = 250 } },
                    { "PageType", p => new Field("PageType", p.Type.ToString(), storedField) },
                    { "Description", p => new Field("Description", MarkdownService.Strip(p.Description), indexedField) }
                };
            }

            public LuceneDocument(Page page)
            {
                Fields = KnownFields.Values.Select(v => v(page)).ToList();
            }

            /// <summary>
            /// Field names and descriptions.
            /// </summary>
            public static readonly Dictionary<string, Func<Page, Field>> KnownFields;

            /// <summary>
            /// Values of a particular page's fields.
            /// </summary>
            public IEnumerable<IIndexableField> Fields { get; }
         
            /// <summary>
            /// Returns all aliases known for a page (including previous names).
            /// </summary>
            private static IEnumerable<string> GetPageAliases(Page page)
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
        }

        #endregion
    }
}