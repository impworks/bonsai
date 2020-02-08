using System;
using System.Diagnostics;
using Bonsai.Data.Models;

namespace Bonsai.Code.Services.Search
{
    /// <summary>
    /// Result of the search.
    /// </summary>
    [DebuggerDisplay("{Id}/{PageType}/{Key}")]
    public class PageDocumentSearchResult
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public PageType PageType { get; set; }

        public string Title { get; set; }
        public string HighlightedTitle { get; set; }
        public string HighlightedDescription { get; set; }
    }
}