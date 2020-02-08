using System;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Relations
{
    /// <summary>
    /// Collection of arguments for a page lookup in relation editor.
    /// </summary>
    public class RelationSuggestQueryVM
    {
        /// <summary>
        /// Search query.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// List of expected page types.
        /// </summary>
        public PageType[] Types { get; set; }

        /// <summary>
        /// Selected relation type.
        /// </summary>
        public RelationType RelationType { get; set; }

        /// <summary>
        /// Selected source page ID (for destination picking).
        /// </summary>
        public Guid? SourceId { get; set; }

        /// <summary>
        /// Selected destination page ID (for source picking).
        /// </summary>
        public Guid? DestinationId { get; set; }
    }
}
