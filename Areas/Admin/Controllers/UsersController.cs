using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.User;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [Area("Admin")]
    [Route("users")]
    public class UsersController: AdminControllerBase
    {
        public UsersController(UserManagerService users, AppDbContext db)
        {
            _users = users;
            _db = db;
        }

        private readonly UserManagerService _users;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the list of users.
        /// </summary>
        [Route("")]
        public async Task<ActionResult> Index()
        {
            var users = await _users.GetUsersListAsync().ConfigureAwait(false);
            ViewBag.Message = TryGetMessage();
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
            return View(vm);
        }

        /// <summary>
        /// Displays the remove confirmation for a user.
        /// </summary>
        [HttpPost]
        [Route("remove")]
        public async Task<ActionResult> Remove(string id, bool confirm)
        {
            await _users.RemoveAsync(id).ConfigureAwait(false);
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
            ViewBag.UserRoles = FormHelper.GetEnumSelectList(vm.Role);
            return View(vm);
        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(UpdateUserVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _users.UpdateAsync(vm).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false);

                return RedirectToSuccess("Пользователь обновлен");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return View(vm);
            }
        }
    }
}
