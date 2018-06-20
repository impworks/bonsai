using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.User;
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
    [Area("Admin")]
    [Route("admin/users")]
    public class UsersController: AdminControllerBase
    {
        public UsersController(UserManagerService users, UserManager<AppUser> userMgr, AppDbContext db)
        {
            _users = users;
            _userMgr = userMgr;
            _db = db;
        }

        private readonly UserManagerService _users;
        private readonly UserManager<AppUser> _userMgr;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the list of users.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index([FromQuery] UserListRequestVM vm)
        {
            var users = await _users.GetUsersListAsync(vm).ConfigureAwait(false);
            return View(users);
        }

        /// <summary>
        /// Displays the remove confirmation for a user.
        /// </summary>
        [HttpGet]
        [Route("remove")]
        public async Task<ActionResult> Remove(string id)
        {
            var vm = await _users.RequestRemoveAsync(id).ConfigureAwait(false);
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
            await _users.RemoveAsync(id, User).ConfigureAwait(false);
            return RedirectToSuccess("Пользователь удален");
        }

        /// <summary>
        /// Displays the update form for a user.
        /// </summary>
        [HttpGet]
        [Route("update")]
        public async Task<ActionResult> Update(string id)
        {
            var vm = await _users.RequestUpdateAsync(id).ConfigureAwait(false);
            return ViewUpdateForm(vm);
        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(UpdateUserVM vm)
        {
            if (!ModelState.IsValid)
                return ViewUpdateForm(vm);

            try
            {
                await _users.UpdateAsync(vm, User).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false);

                return RedirectToSuccess("Пользователь обновлен");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return ViewUpdateForm(vm);
            }
        }

        #region Helpers

        /// <summary>
        /// Displays the UpdateUser form.
        /// </summary>
        private ActionResult ViewUpdateForm(UpdateUserVM vm)
        {
            ViewBag.IsSelf = _users.IsSelf(vm.Id, User);
            ViewBag.UserRoles = ViewHelper.GetEnumSelectList(vm.Role);

            return View("Update", vm);
        }

        #endregion
    }
}
