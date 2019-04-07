using System;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Front.ViewModels.Tree
{
    /// <summary>
    /// Information about a rendered tree.
    /// </summary>
    public class TreeVM
    {
        /// <summary>
        /// ID of the root element.
        /// </summary>
        public Guid RootId { get; set; }

        /// <summary>
        /// Complete layout.
        /// </summary>
        public JObject Content { get; set; }
    }
}
