using System;
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
using Impworks.Utils.Strings;
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
                Events = await GetEventsAsync().ToListAsync(),
                PagesCount = await _db.Pages.CountAsync(x => x.IsDeleted == false),
                PagesToImproveCount = await _db.PagesScored.CountAsync(x => x.IsDeleted == false && x.CompletenessScore <= 50),
                MediaCount = await _db.Media.CountAsync(x => x.IsDeleted == false),
                MediaToTagCount = await _db.Media.CountAsync(x => x.IsDeleted == false && !x.Tags.Any()),
                RelationsCount = await _db.Relations.CountAsync(x => x.IsComplementary == false && x.IsDeleted == false)
            };
        }

        /// <summary>
        /// Returns the list of changeset groups at given offset.
        /// </summary>
        public async IAsyncEnumerable<ChangesetEventVM> GetEventsAsync(int page = 0)
        {
            const int PAGE_SIZE = 20;

            var groups = await _db.ChangeEvents
                                  .OrderByDescending(x => x.Date)
                                  .Skip(PAGE_SIZE * page)
                                  .Take(PAGE_SIZE)
                                  .ToListAsync();

            var parsedGroups = groups.Select(x => new {x.GroupKey, Ids = x.Ids.Split(',').Select(y => y.Parse<Guid>())})
                                     .ToList();

            var changeIds = parsedGroups.SelectMany(x => x.Ids).ToList();
            var changes = await _db.Changes
                                   .AsNoTracking()
                                   .Include(x => x.EditedMedia)
                                   .Include(x => x.EditedPage)
                                   .Include(x => x.EditedRelation)
                                   .Where(x => changeIds.Contains(x.Id))
                                   .ToDictionaryAsync(x => x.Id, x => x);

            foreach (var group in parsedGroups)
            {
                var firstChange = changes[group.Ids.First()];
                var vm = _mapper.Map<ChangesetEventVM>(firstChange);

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
                    vm.MediaThumbnails = group.Ids.Select(x => _mapper.Map<MediaThumbnailVM>(changes[x].EditedMedia)).ToList();
                }

                yield return vm;
            }
        }
    }
}
