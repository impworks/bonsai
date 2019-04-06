using System.Linq;
using System.Threading.Tasks;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// The presenter for tree elements.
    /// </summary>
    public class TreePresenterService
    {
        #region Constructor

        public TreePresenterService(AppDbContext db, IUrlHelper url)
        {
            _db = db;
            _url = url;
        }

        #endregion

        #region Fields

        private readonly AppDbContext _db;
        private readonly IUrlHelper _url;

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

            var json = page?.TreeLayout?.LayoutJson;
            if (string.IsNullOrEmpty(json))
                return null;

            var data = JObject.Parse(json);
            foreach (var child in data["children"])
            {
                var info = child["info"];
                if(info == null)
                    continue;

                info["Photo"] = _url.Content(info["Photo"].Value<string>());
                info["Url"] = _url.Action("Description", "Page", new {area = "Front", key = info["Url"].Value<string>()});
            }

            return page.TreeLayout?.LayoutJson;
        }

        #endregion
    }
}
