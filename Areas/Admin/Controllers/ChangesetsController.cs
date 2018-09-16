using System;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Areas.Admin.ViewModels.Changesets;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Data;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing changesets.
    /// </summary>
    [Route("admin/changes")]
    public class ChangesetsController: AdminControllerBase
    {
        public ChangesetsController(ChangesetsManagerService changes, MediaManagerService media, PagesManagerService pages, RelationsManagerService rels, AppDbContext db)
        {
            _changes = changes;
            _media = media;
            _pages = pages;
            _rels = rels;
            _db = db;
        }

        private readonly ChangesetsManagerService _changes;
        private readonly MediaManagerService _media;
        private readonly PagesManagerService _pages;
        private readonly RelationsManagerService _rels;
        private readonly AppDbContext _db;

        #region Public methods

        /// <summary>
        /// Displays the list of all changesets.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(ChangesetsListRequestVM request)
        {
            ViewBag.Data = await GetDataAsync(request);
            var vm = await _changes.GetChangesetsAsync(request);
            return View(vm);
        }

        /// <summary>
        /// Displays the information about a particular changeset.
        /// </summary>
        [HttpGet]
        [Route("details")]
        public async Task<ActionResult> Details(Guid id)
        {
            var vm = await _changes.GetChangesetDetailsAsync(id);
            return View(vm);
        }

        /// <summary>
        /// Displays the changeset revert confirmation.
        /// </summary>
        [HttpGet]
        [Route("revert")]
        public async Task<ActionResult> Revert(Guid id)
        {
            var vm = await _changes.GetChangesetDetailsAsync(id);
            return View(vm);
        }

        /// <summary>
        /// Reverts the edit to its original state.
        /// </summary>
        [HttpPost]
        [Route("revert")]
        public async Task<ActionResult> Revert(Guid id, bool confirm)
        {
            var vm = await _changes.GetReverseEditorStateAsync(id);

            if (vm is MediaEditorVM mvm)
                await _media.UpdateAsync(mvm, User);
            else if (vm is PageEditorVM pvm)
                await _pages.UpdateAsync(pvm, User);
            else if (vm is RelationEditorVM rvm)
                await _rels.UpdateAsync(rvm, User);

            await _db.SaveChangesAsync();

            return RedirectToSuccess("Правка была отменена");
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// 
        /// </summary>
        private async Task<ChangesetsListDataVM> GetDataAsync(ChangesetsListRequestVM request)
        {
            var data = new ChangesetsListDataVM();

            if (!string.IsNullOrEmpty(request.UserId))
            {
                var user = await _db.Users
                                    .Where(x => x.Id == request.UserId)
                                    .Select(x => new {x.FirstName, x.LastName})
                                    .FirstOrDefaultAsync();

                if (user != null)
                    data.UserTitle = user.FirstName + " " + user.LastName;
                else
                    request.UserId = null;
            }

            if (request.EntityId != null)
            {
                var title = await GetPageTitleAsync()
                            ?? await GetMediaTitleAsync();

                if (title != null)
                    data.EntityTitle = title;
                else
                    request.EntityId = null;
            }

            return data;

            async Task<string> GetPageTitleAsync()
            {
                return await _db.Pages
                                .Where(x => x.Id == request.EntityId)
                                .Select(x => x.Title)
                                .FirstOrDefaultAsync();
            }

            async Task<string> GetMediaTitleAsync()
            {
                var media = await _db.Media
                                     .Where(x => x.Id == request.EntityId)
                                     .Select(x => new {Title = x.Title})
                                     .FirstOrDefaultAsync();

                return media == null
                    ? null
                    : StringHelper.Coalesce(media.Title, "Медиа");
            }
        }

        #endregion
    }
}
