using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.Users;
using Bonsai.Data;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for displaying dashboard info.
    /// </summary>
    public class DashboardPresenterService
    {
        public DashboardPresenterService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        /// <summary>
        /// Returns the dashboard data.
        /// </summary>
        public async Task<DashboardVM> GetDashboardAsync()
        {
            var pages = await GetLastUpdatedPagesAsync(5);
            var media = await GetLastUploadedMediaAsync(5);
            var users = await GetNewUsersAsync();

            return new DashboardVM
            {
                UpdatedPages = pages,
                UploadedMedia = media,
                NewUsers = users
            };
        }

        #region Private helpers

        /// <summary>
        /// Returns the last X updated pages (for front page).
        /// </summary>
        private async Task<IReadOnlyList<PageTitleExtendedVM>> GetLastUpdatedPagesAsync(int count)
        {
            return await _db.Pages
                            .OrderByDescending(x => x.LastUpdateDate)
                            .Take(count)
                            .ProjectTo<PageTitleExtendedVM>(_mapper.ConfigurationProvider)
                            .ToListAsync();
        }
        
        /// <summary>
        /// Returns the last X uploaded media files (for front page).
        /// </summary>
        private async Task<IReadOnlyList<MediaThumbnailExtendedVM>> GetLastUploadedMediaAsync(int count)
        {
            return await _db.Media
                            .OrderByDescending(x => x.UploadDate)
                            .Take(count)
                            .ProjectTo<MediaThumbnailExtendedVM>(_mapper.ConfigurationProvider)
                            .ToListAsync();
        }

        /// <summary>
        /// Returns users that are not yet validated.
        /// </summary>
        private async Task<IReadOnlyList<UserTitleVM>> GetNewUsersAsync()
        {
            return await _db.Users
                            .Where(x => x.IsValidated == false)
                            .ProjectTo<UserTitleVM>(_mapper.ConfigurationProvider)
                            .OrderBy(x => x.FullName)
                            .ToListAsync();
        }

        #endregion
    }
}
