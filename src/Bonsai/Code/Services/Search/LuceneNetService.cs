using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using Page = Bonsai.Data.Models.Page;

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
            
            var formatter = new SimpleHTMLFormatter("<b>", "</b>");
            var scorer = new QueryScorer(query);
            var highlighter = new Highlighter(formatter, scorer) { TextFragmenter = new SimpleSpanFragmenter(scorer, 150) };

            var results = new List<PageDocumentSearchResult>();

            var searchResultsDocuments = documents.Skip(PAGE_SIZE * page).Take(PAGE_SIZE).ToList();
            
            foreach (var doc in searchResultsDocuments)
            {
                var description = doc.Get("Description");
                var title = doc.Get("Title");

                using var descriptionStream = _writer.Analyzer.GetTokenStream("Description", description);
                var highlightedDescription = highlighter.GetBestFragments(descriptionStream, description, 1).FirstOrDefault() ?? "";

                using var titleStream =  _writer.Analyzer.GetTokenStream("Title", title);
                var highlightedTitle = highlighter.GetBestFragments(titleStream, title, 1).FirstOrDefault() ?? "";

                results.Add(new PageDocumentSearchResult
                {
                    Id = Guid.Parse(doc.Get("Id")),
                    Key = doc.Get("Key"),
                    PageType = (PageType) Convert.ToInt32(doc.Get("PageType")),
                    HighlightedTitle = highlightedTitle,
                    HighlightedDescription = WrapHighlight(highlightedDescription, description)
                });
            }
            
            return Task.FromResult((IReadOnlyList<PageDocumentSearchResult>) results);
        }

        /// <summary>
        /// Returns the suggestion list for the search field.
        /// </summary>
        public Task<IReadOnlyList<PageDocumentSearchResult>> SuggestAsync(string phrase, IReadOnlyList<PageType> pageTypes = null, int? maxCount = null)
        {
            var (documents, _) = SearchIndex(phrase, pageTypes, maxCount, true);

            var results = documents.Select(document => new PageDocumentSearchResult
            {
                Id = Guid.Parse(document.Get("Id")),
                Key = document.Get("Key"),
                HighlightedTitle = document.Get("Title"), // todo: highlight here and use on the client
                PageType = (PageType) Convert.ToInt32(document.Get("PageType"))
            }).ToList();

            return Task.FromResult((IReadOnlyList<PageDocumentSearchResult>) results);        
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Partitions the search phrase to separate terms.
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
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
            phrase = phrase.ToLower();
            
            using var directoryReader = _writer.GetReader(true);
            var searcher = new IndexSearcher(directoryReader);
            searcher.Similarity = new LMJelinekMercerSimilarity(0.2f);

            var words = SplitTerms(phrase).ToList();

            var booleanQuery = new BooleanQuery();

            var fields = new [] {"Title", "Description", "Aliases"};

            for (int i = 0; i < words.Count; i++)
            {
                var boostBase = (words.Count - i + 1.0f) * 3;
                foreach (var f in fields)
                {
                    var term = new Term(f, words[i]);
                    booleanQuery.Add(new FuzzyQuery(term, 2, 0) {Boost = boostBase}, Occur.SHOULD);
                    if (!suggest)
                    {
                        var phraseQuery = new PhraseQuery();
                        phraseQuery.Add(term);
                        phraseQuery.Boost = boostBase * 3;
                        phraseQuery.Slop = 2;
                        booleanQuery.Add(phraseQuery, Occur.SHOULD);
                    }
                    else
                    {
                        booleanQuery.Add(new PrefixQuery(term) { Boost = boostBase * 3 }, Occur.SHOULD);
                    }
                }
            }

            if (pageTypes != null)
            {
                var pageTypesQueries = pageTypes.Select(v => new TermQuery(new Term("PageType", ((int) v).ToString())));

                foreach (var pageTypesQuery in pageTypesQueries)
                    booleanQuery.Add(pageTypesQuery, Occur.MUST);
            }

            var searchResults = searcher.Search(booleanQuery, maxCount ?? int.MaxValue);
            var documents = searchResults.ScoreDocs.Select(v => directoryReader.Document(v.Doc)).ToList();
            
            return (documents, booleanQuery);
        }

        private string WrapHighlight(string highlighted, string actual)
        {
            var bare = highlighted.Replace("<b>", "").Replace("</b>", "");
            var startEllipsis = !actual.StartsWithPart(bare, 10);
            var endEllipsis = !actual.EndsWithPart(bare, 10);

            return (startEllipsis ? "..." : "") + highlighted + (endEllipsis ? "..." : "");
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
                var storedField = new FieldType {IsStored = true};

                KnownFields = new Dictionary<string, Func<PageDocument, Field>>
                {
                    { "Id", p => new Field("Id", p.Id.ToString(), storedField) },
                    { "Key", p => new Field("Key", p.Key, indexedField) },
                    { "Title", p => new Field("Title", p.Title, indexedField) { Boost = 500 } },
                    { "Aliases", p => new Field("Aliases", p.Aliases, indexedField) { Boost = 250 } },
                    { "PageType", p => new Field("PageType", p.PageType.ToString(), storedField) },
                    { "Description", p => new Field("Description", p.Description, indexedField) },
                };
            }

            public LuceneDocument(Page page)
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

                Fields = KnownFields.Values.Select(v => v(doc)).ToList();
            }

            /// <summary>
            /// Field names and descriptions.
            /// </summary>
            public static readonly Dictionary<string, Func<PageDocument, Field>> KnownFields;

            /// <summary>
            /// Values of a particular page's fields.
            /// </summary>
            public IEnumerable<IIndexableField> Fields { get; }
         
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
        }

        #endregion
    }
}