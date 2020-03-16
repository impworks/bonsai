using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for displaying dashboard info.
    /// </summary>
    public class DashboardPresenterService
    {
        public DashboardPresenterService(AppDbContext db, IUrlHelper url, IMapper mapper)
        {
            _db = db;
            _url = url;
            _mapper = mapper;
        }

        private readonly AppDbContext _db;
        private readonly IUrlHelper _url;
        private readonly IMapper _mapper;

        /// <summary>
        /// Returns the dashboard data.
        /// </summary>
        public async Task<DashboardVM> GetDashboardAsync()
        {
            return new DashboardVM
            {
                Changesets = await GetChangesetGroupsAsync().ToListAsync(),
                PagesCount = await _db.Pages.CountAsync(),
                PagesToImproveCount = await _db.PagesScored.CountAsync(x => x.CompletenessScore <= 50),
                MediaCount = await _db.Media.CountAsync(),
                MediaToTagCount = await _db.Media.CountAsync(x => !x.Tags.Any()),
                RelationsCount = await _db.Relations.CountAsync()
            };
        }

        /// <summary>
        /// Returns the list of changeset groups at given offset.
        /// </summary>
        public async IAsyncEnumerable<ChangesetGroupVM> GetChangesetGroupsAsync(int page = 0)
        {
            const int PAGE_SIZE = 20;

            var elems = await _db.ChangesGrouped
                                 .AsNoTracking()
                                 .Include(x => x.EditedMedia)
                                 .Include(x => x.EditedPage)
                                 .Include(x => x.EditedRelation)
                                 .GroupBy(x => x.GroupKey)
                                 .Skip(PAGE_SIZE * page)
                                 .Take(PAGE_SIZE)
                                 .ToListAsync();

            foreach (var elem in elems)
            {
                var firstChange = elem.First();
                var vm = _mapper.Map<ChangesetGroupVM>(firstChange);

                if (firstChange.Type == ChangesetEntityType.Page)
                {
                    vm.Title = firstChange.EditedPage.Title;
                    vm.Url = _url.Action("Description", "Page", new {area = "Front", key = firstChange.EditedPage.Key});
                }
                else if (firstChange.Type == ChangesetEntityType.Relation)
                {
                    vm.Title = "связь " + firstChange.EditedRelation.Type.GetEnumDescription();
                }
                else if (firstChange.Type == ChangesetEntityType.Media)
                {
                    vm.MediaThumbnails = elem.Select(x => _mapper.Map<MediaThumbnailVM>(x.EditedMedia)).ToList();
                }

                yield return vm;
            }
        }
    }
}
