using System;
using Bonsai.Data.Models;

namespace Bonsai.Code.Services.Search
{
    /// <summary>
    /// Result of the search.
    /// </summary>
    public class PageDocumentSearchResult
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public PageType PageType { get; set; }

        public string HighlightedTitle { get; set; }
        public string HighlightedDescription { get; set; }
    }
}