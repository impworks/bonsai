using System;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Data;

namespace Bonsai.Areas.Front.Logic.Tree
{
    /// <summary>
    /// The presenter for tree elements.
    /// </summary>
    public class TreePresenterService
    {
        #region Constructor

        public TreePresenterService(AppDbContext db)
        {
            _db = db;
        }

        #endregion

        #region Fields

        private readonly AppDbContext _db;

        #endregion

        #region Public methods

        /// <summary>
        /// Returns the entire tree.
        /// </summary>
        public async Task<TreeVM> GetTreeAsync(Guid pageId)
        {
            var relContext = await RelationContext.LoadContextAsync(_db, new RelationContextOptions { PeopleOnly = true });
            var treeContext = new TreeContext(relContext, pageId);
            return treeContext.GetTree();
        }

        #endregion
    }
}
