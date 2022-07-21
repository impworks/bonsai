using System;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Inner link from the description of one page (in markdown code) to another page.
    /// </summary>
    public class PageReference
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Page containing the reference.
        /// </summary>
        public Page Source { get; set; }
        public Guid SourceId { get; set; }
        
        /// <summary>
        /// Page referenced in the text.
        /// </summary>
        public Page Destination { get; set; }
        public Guid DestinationId { get; set; }
    }
}