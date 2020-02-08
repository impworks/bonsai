using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Tree;
using Bonsai.Code.Utils.Helpers;
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
        public async Task<TreeVM> GetTreeAsync(string key)
        {
            var keyLower = key?.ToLowerInvariant();

            var page = await _db.Pages
                                .AsNoTracking()
                                .Include(x => x.TreeLayout)
                                .GetAsync(x => x.Aliases.Any(y => y.Key == keyLower) && x.IsDeleted == false, "Страница не найдена");

            var result = new TreeVM {RootId = page.Id};
            var json = page?.TreeLayout?.LayoutJson;
            if (!string.IsNullOrEmpty(json))
            {
                result.Content = JObject.Parse(json);
                foreach (var child in result.Content["children"])
                {
                    var info = child["info"];
                    if (info == null)
                        continue;

                    info["Photo"] = _url.Content(info["Photo"].Value<string>());
                    info["Url"] = _url.Action("Description", "Page", new {area = "Front", key = info["Url"].Value<string>()});
                }
            }

            return result;
        }

        #endregion
    }
}
