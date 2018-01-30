using System;

namespace Bonsai.Code.Services.Elastic
{
    /// <summary>
    /// Result of the search.
    /// </summary>
    public class PageDocumentSearchResult
    {
        public Guid PageId { get; set; }

        public double NormalizedScore { get; set; }

        public string HighlightedTitle { get; set; }
        public string HighlightedDescription { get; set; }
    }
}
