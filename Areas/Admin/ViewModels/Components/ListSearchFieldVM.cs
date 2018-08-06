using System.Collections.Generic;

namespace Bonsai.Areas.Admin.ViewModels.Components
{
    /// <summary>
    /// Details for a list search form.
    /// </summary>
    public class ListSearchFieldVM
    {
        /// <summary>
        /// Request URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The search query's current value.
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// The other values for current search (order, etc.).
        /// </summary>
        public Dictionary<string, string> OtherValues { get; set; }
    }
}
