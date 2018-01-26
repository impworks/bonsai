using System;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Search;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// The presenter for search results.
    /// </summary>
    public class SearchPresenterService
    {
        public SearchPresenterService(AppDbContext db, IUrlHelper url)
        {
            _db = db;
            _url = url;
        }

        private readonly AppDbContext _db;
        private readonly IUrlHelper _url;

        /// <summary>
        /// Find pages matching the query.
        /// </summary>
        public async Task<SearchVM> SearchAsync(string query)
        {
            throw new NotImplementedException();
        }
    }
}
