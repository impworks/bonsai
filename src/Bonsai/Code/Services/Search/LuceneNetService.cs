using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Sandbox.Queries;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using Lucene.Net.Tartarus.Snowball.Ext;
using Lucene.Net.Util;
using Newtonsoft.Json.Linq;
using Page = Bonsai.Data.Models.Page;

namespace Bonsai.Code.Services.Search
{
    public class LuceneNetService : ISearchEngine, IAsyncDisposable
    {
        private readonly IndexWriter _writer;

        public LuceneNetService()
        {
            var luceneVersion = LuceneVersion.LUCENE_48;
            var russianAnalyzer = new RussianAnalyzer(luceneVersion);
            var indexConfig = new IndexWriterConfig(luceneVersion, russianAnalyzer);
            
            _writer = new IndexWriter(new RAMDirectory(), indexConfig);
        }
        
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task AddPageAsync(Page page)
        {
            var luceneDoc = new LuceneDocument(page);
            _writer.AddDocument(luceneDoc.Fields);
            _writer.Commit();
            return Task.CompletedTask;
        }

        public Task RemovePageAsync(Page page)
        {
            var query = new TermQuery(new Term("Id", page.Id.ToString()));
            _writer.DeleteDocuments(query);
            _writer.Commit();
            return Task.CompletedTask;
        }

        public Task ClearDataAsync()
        {
            _writer.DeleteAll();
            _writer.Commit();
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<PageDocumentSearchResult>> SearchAsync(string phrase, int page = 0)
        {
            var searchResults = SearchIndex(phrase);
            
            // create highlighter
            var formatter = new SimpleHTMLFormatter("<b>", "</b>");
            var scorer = new QueryScorer(searchResults.booleanQuery);

            var highlighter = new Highlighter(formatter, scorer) {TextFragmenter = new NullFragmenter()};

            var results = new List<PageDocumentSearchResult>();
            foreach (var doc in searchResults.documents)
            {
                var description = doc.Get("Description");
                var title = doc.Get("Title");

                var stream = _writer.Analyzer.GetTokenStream("Description", new StringReader(description));
                var highlightedDescription = highlighter.GetBestFragments(stream, description, 50).JoinString(", ");
                
                stream =  _writer.Analyzer.GetTokenStream("Title", new StringReader(title));
                var highlightedTitle = highlighter.GetBestFragments(stream, title, 50).JoinString(", ");

                results.Add(new PageDocumentSearchResult
                {
                    Id = Guid.Parse(doc.Get("Id")),
                    Key = doc.Get("Key"),
                    PageType = (PageType) Convert.ToInt32(doc.Get("PageType")),
                    HighlightedTitle = highlightedTitle,
                    HighlightedDescription = highlightedDescription

                });
            }

            return Task.FromResult((IReadOnlyList<PageDocumentSearchResult>) results);
        }

        public Task<IReadOnlyList<PageDocumentSearchResult>> SuggestAsync(string phrase, IReadOnlyList<PageType> pageTypes = null, int? maxCount = null)
        {
            var searchResults = SearchIndex(phrase, pageTypes, maxCount);

            var results = searchResults.documents.Select(document => new PageDocumentSearchResult
            {
                Id = Guid.Parse(document.Get("Id")),
                Key = document.Get("Key"),
                PageType = (PageType) Convert.ToInt32(document.Get("PageType"))
            }).ToList();

            return Task.FromResult((IReadOnlyList<PageDocumentSearchResult>) results);        
        }

        private IEnumerable<string> SplitTerms(string phrase)
        {
            int i = 0;
            var lastI = 0;
            for (i = 0; i < phrase.Length; i++)
            {
                if (!char.IsLetter(phrase[i]))
                {
                    var substring = phrase.Substring(lastI, i - lastI);
                    if (!string.IsNullOrWhiteSpace(substring))
                        yield return substring;
                    lastI = i + 1;
                }
            }

            yield return phrase.Substring(lastI);
        }
        
        private (TopDocs searchResults, List<Document> documents, Query booleanQuery) SearchIndex(string phrase, IReadOnlyList<PageType> pageTypes = null, int? maxCount = null)
        {
            phrase = phrase.ToLower();
            
            using var directoryReader = _writer.GetReader(true);
            var searcher = new IndexSearcher(directoryReader);

            var words = SplitTerms(phrase).ToList();

            var booleanQuery = new BooleanQuery();

            for (int i = 0; i < words.Count; i++)
            {
                var q1 = new FuzzyQuery(new Term("Title", words[i]), 2, 0) {Boost = words.Count * 2 + i + 1};
                var q2 = new FuzzyQuery(new Term("Description", words[i]), 2, 0) { Boost = i + 1};
                var q3 = new FuzzyQuery(new Term("Aliases", words[i]), 2, 0) { Boost = i + 1};
                booleanQuery.Add(q1, Occur.SHOULD);
                booleanQuery.Add(q2, Occur.SHOULD);
                booleanQuery.Add(q3, Occur.SHOULD);
            }

            if (pageTypes != null)
            {
                var pageTypesQueries = pageTypes.Select(v => new TermQuery(new Term("PageType", ((int) v).ToString())));

                foreach (var pageTypesQuery in pageTypesQueries)
                {
                    booleanQuery.Add(pageTypesQuery, Occur.MUST);
                }
            }

            var searchResults = searcher.Search(booleanQuery, maxCount ?? Int32.MaxValue);
            var documents = searchResults.ScoreDocs.Select(v => directoryReader.Document(v.Doc)).ToList();
            
            return (searchResults, documents, booleanQuery);
        }
        
        

        public ValueTask DisposeAsync()
        {
            _writer.Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        private class LuceneDocument
        {
            public static readonly Dictionary<string, Func<PageDocument, Field>> KnownFields;

            static LuceneDocument()
            {
                var indexedField = new FieldType() {IsIndexed = true, IsStored = true, IsTokenized = true };
                var storedField = new FieldType() {IsStored = true};

                KnownFields = new Dictionary<string, Func<PageDocument, Field>>
                {
                    { "Id", p => new Field("Id", p.Id.ToString(), storedField) },
                    { "Key", p => new Field("Key", p.Key, indexedField) },
                    { "Title", p => new Field("Title", p.Title, indexedField) },
                    { "Aliases", p => new Field("Aliases", p.Aliases, indexedField) },
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
    }
}