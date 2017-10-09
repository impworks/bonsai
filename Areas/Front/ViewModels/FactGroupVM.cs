using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels
{
    /// <summary>
    /// Group of related facts.
    /// </summary>
    public class FactGroupVM
    {
        /// <summary>
        /// Readable fact group title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The set of fact data.
        /// </summary>
        public IEnumerable<FactVM> Facts { get; set; }
    }
}
