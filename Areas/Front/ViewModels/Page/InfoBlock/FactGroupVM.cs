using System.Collections.Generic;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Models;

namespace Bonsai.Areas.Front.ViewModels.Page.InfoBlock
{
    /// <summary>
    /// Group of related facts.
    /// </summary>
    public class FactGroupVM
    {
        /// <summary>
        /// Definition of the group.
        /// </summary>
        public FactDefinitionGroup Definition { get; set; }

        /// <summary>
        /// The set of fact data.
        /// </summary>
        public ICollection<FactModelBase> Facts { get; set; }
    }
}
