using System;
using Bonsai.Data.Models;

namespace Bonsai.Code.Services.Elastic
{
    /// <summary>
    /// The page's searchable fields.
    /// </summary>
    public class PageDocument
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Extracts the fields from a database-driven page definition.
        /// </summary>
        public static explicit operator PageDocument(Page page)
        {
            return new PageDocument
            {
                Id = page.Id,
                Title = page.Title,
                Description = page.Description
            };
        }
    }
}
