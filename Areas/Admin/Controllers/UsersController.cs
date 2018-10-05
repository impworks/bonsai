using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Users;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [Route("admin/users")]
    public class UsersController: AdminControllerBase
    {
        public UsersController(UsersManagerService users, PagesManagerService pages, UserManager<AppUser> userMgr, AppDbContext db)
        {
            _users = users;
            _pages = pages;
            _userMgr = userMgr;
            _db = db;
        }

        private readonly UsersManagerService _users;
        private readonly PagesManagerService _pages;
        private readonly UserManager<AppUser> _userMgr;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the list of users.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index([FromQuery] UsersListRequestVM vm)
        {
            var users = await _users.GetUsersAsync(vm);
            return View(users);
        }

        /// <summary>
        /// Displays the remove confirmation for a user.
        /// </summary>
        [HttpGet]
        [Route("remove")]
        public async Task<ActionResult> Remove(string id)
        {
            var vm = await _users.RequestRemoveAsync(id);
            ViewBag.IsSelf = _userMgr.GetUserId(User) == id;
            return View(vm);
        }

        /// <summary>
        /// Displays the remove confirmation for a user.
        /// </summary>
        [HttpPost]
        [Route("remove")]
        public async Task<ActionResult> Remove(string id, bool confirm)
        {
            await _users.RemoveAsync(id, User);
            return RedirectToSuccess("Пользователь удален");
        }

        /// <summary>
        /// Displays the update form for a user.
        /// </summary>
        [HttpGet]
        [Route("update")]
        public async Task<ActionResult> Update(string id)
        {
            var vm = await _users.RequestUpdateAsync(id);
            return await ViewUpdateFormAsync(vm);
        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(UserEditorVM vm)
        {
            if (!ModelState.IsValid)
                return await ViewUpdateFormAsync(vm);

            try
            {
                await _users.UpdateAsync(vm, User);

                if (vm.CreatePersonalPage && await _users.CanCreatePersonalPageAsync(vm))
                    await _pages.CreateDefaultUserPageAsync(vm, User);

                await _db.SaveChangesAsync();

                return RedirectToSuccess("Пользователь обновлен");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return await ViewUpdateFormAsync(vm);
            }
        }

        #region Helpers

        /// <summary>
        /// Displays the UpdateUser form.
        /// </summary>
        private async Task<ActionResult> ViewUpdateFormAsync(UserEditorVM vm)
        {
            ViewBag.Data = new UserEditorDataVM
            {
                IsSelf = _users.IsSelf(vm.Id, User),
                UserRoles = ViewHelper.GetEnumSelectList(vm.Role),
                CanCreatePersonalPage = await _users.CanCreatePersonalPageAsync(vm)
            };

            return View("Update", vm);
        }

        #endregion
    }
}
