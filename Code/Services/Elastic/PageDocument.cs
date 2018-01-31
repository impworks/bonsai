using System;

namespace Bonsai.Code.Services.Elastic
{
    /// <summary>
    /// The page's searchable fields.
    /// </summary>
    public class PageDocument
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
