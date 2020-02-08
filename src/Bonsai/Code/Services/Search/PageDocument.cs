using System;

namespace Bonsai.Code.Services.Search
{
    /// <summary>
    /// The page's searchable fields.
    /// </summary>
    public class PageDocument
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Aliases { get; set; }
        public string Description { get; set; }
        public int PageType { get; set; }
    }
}
