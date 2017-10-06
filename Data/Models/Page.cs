using System;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Instance of a page.
    /// </summary>
    public class Page
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Url { get; set; }

        public PageType EntityType { get; set; }
        public string Description { get; set; }
        public string Facts { get; set; }
    }
}
