using System.Collections.Generic;
using Bonsai.Code.DomainModel.Facts.Models;

namespace Bonsai.Areas.Front.ViewModels.Page.InfoBlock
{
    /// <summary>
    /// Group of related facts.
    /// </summary>
    public class FactGroupVM
    {
        /// <summary>
        /// ID of the fact group.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Readable fact group title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The set of fact data.
        /// </summary>
        public ICollection<FactModelBase> Facts { get; set; }
    }
}
