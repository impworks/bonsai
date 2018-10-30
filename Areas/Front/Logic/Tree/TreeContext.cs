using System;
using Bonsai.Areas.Front.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;

namespace Bonsai.Areas.Front.Logic.Tree
{
    /// <summary>
    /// The context for building a relation tree.
    /// </summary>
    public class TreeContext
    {
        #region Constructor

        public TreeContext(RelationContext rels, Guid root)
        {
            _rels = rels;
            _rootId = root;
        }

        #endregion

        #region Fields

        private readonly RelationContext _rels;
        private readonly Guid _rootId;

        #endregion

        #region Methods

        /// <summary>
        /// Returns the built tree.
        /// </summary>
        public TreeVM GetTree()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
