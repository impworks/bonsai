using System.Linq;
using System.Threading.Tasks;
using Bonsai.Data;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic
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
        public async Task<string> GetTreeAsync(string key)
        {
            var keyLower = key?.ToLowerInvariant();

            var page = await _db.Pages
                                .AsNoTracking()
                                .Include(x => x.TreeLayout)
                                .FirstOrDefaultAsync(x => x.Aliases.Any(y => y.Key == keyLower) && x.IsDeleted == false);

            return page.TreeLayout?.LayoutJson;
        }

        #endregion
    }
}
