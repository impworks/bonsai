using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.User;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for displaying dashboard info.
    /// </summary>
    public class DashboardPresenterService
    {
        public DashboardPresenterService(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        /// <summary>
        /// Returns the dashboard data.
        /// </summary>
        public async Task<DashboardVM> GetDashboardAsync()
        {
            var pages = await GetLastUpdatedPagesAsync(5).ConfigureAwait(false);
            var media = await GetLastUploadedMediaAsync(5).ConfigureAwait(false);
            var users = await GetNewUsersAsync().ConfigureAwait(false);

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
                            .Select(x => new PageTitleExtendedVM
                            {
                                Title = x.Title,
                                Key = x.Key,
                                Type = x.PageType,
                                UpdatedDate = x.LastUpdateDate.LocalDateTime,
                                MainPhotoPath = x.MainPhoto != null
                                    ? MediaPresenterService.GetSizedMediaPath(x.MainPhoto.FilePath, MediaSize.Small)
                                    : null
                            })
                            .ToListAsync()
                            .ConfigureAwait(false);
        }

        
        /// <summary>
        /// Returns the last X uploaded media files (for front page).
        /// </summary>
        private async Task<IReadOnlyList<MediaThumbnailExtendedVM>> GetLastUploadedMediaAsync(int count)
        {
            return await _db.Media
                            .OrderByDescending(x => x.UploadDate)
                            .Take(count)
                            .Select(x => new MediaThumbnailExtendedVM
                            {
                                Type = x.Type,
                                MediaKey = x.Key,
                                ThumbnailUrl = MediaPresenterService.GetSizedMediaPath(x.FilePath, MediaSize.Small),
                                Date = FuzzyDate.TryParse(x.Date),
                                UploadDate = x.UploadDate
                            })
                            .ToListAsync()
                            .ConfigureAwait(false);
        }

        /// <summary>
        /// Returns users that are not yet validated.
        /// </summary>
        private async Task<IReadOnlyList<UserTitleVM>> GetNewUsersAsync()
        {
            return await _db.Users
                            .Where(x => x.IsValidated == false)
                            .Select(x => new UserTitleVM
                            {
                                Id = x.Id,
                                FullName = x.FirstName + " " + x.LastName
                            })
                            .OrderBy(x => x.FullName)
                            .ToListAsync()
                            .ConfigureAwait(false);
        }

        #endregion
    }
}
