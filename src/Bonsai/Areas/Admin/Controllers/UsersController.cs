using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Users;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Search;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [Route("admin/users")]
    public class UsersController : AdminControllerBase
    {
        public UsersController(UsersManagerService users, PagesManagerService pages, BonsaiConfigService config, ISearchEngine search, AppDbContext db)
        {
            _users = users;
            _pages = pages;
            _config = config;
            _search = search;
            _db = db;
        }

        private readonly UsersManagerService _users;
        private readonly PagesManagerService _pages;
        private readonly BonsaiConfigService _config;
        private readonly ISearchEngine _search;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the list of users.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index([FromQuery] UsersListRequestVM vm)
        {
            var users = await _users.GetUsersAsync(vm);
            ViewBag.AllowPasswordAuth = _config.GetStaticConfig().Auth.AllowPasswordAuth;
            return View(users);
        }

        /// <summary>
        /// Displays the remove confirmation for a user.
        /// </summary>
        [HttpGet]
        [Route("remove")]
        public async Task<ActionResult> Remove(string id)
        {
            var vm = await _users.RequestRemoveAsync(id, User);
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
                if (vm.CreatePersonalPage && await _users.CanCreatePersonalPageAsync(vm))
                {
                    var page = await _pages.CreateDefaultUserPageAsync(vm, User);
                    vm.PersonalPageId = page.Id;
                    vm.CreatePersonalPage = false;

                    await _search.AddPageAsync(page);
                }

                await _users.UpdateAsync(vm, User);

                await _db.SaveChangesAsync();

                return RedirectToSuccess("Пользователь обновлен");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return await ViewUpdateFormAsync(vm);
            }
        }

        /// <summary>
        /// Displays the form for creating a new user.
        /// </summary>
        [HttpGet]
        [Route("create")]
        public async Task<ActionResult> Create()
        {
            CheckPasswordAuth();

            return await ViewCreateFormAsync(new UserCreatorVM { Role = UserRole.User });
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> Create(UserCreatorVM vm)
        {
            CheckPasswordAuth();

            if (!ModelState.IsValid)
                return await ViewCreateFormAsync(vm);

            try
            {
                if (vm.CreatePersonalPage)
                {
                    var page = await _pages.CreateDefaultUserPageAsync(vm, User);
                    vm.PersonalPageId = page.Id;
                    vm.CreatePersonalPage = false;
                    
                    await _search.AddPageAsync(page);
                }

                await _users.CreateAsync(vm);
                await _db.SaveChangesAsync();

                return RedirectToSuccess("Пользователь создан");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return await ViewCreateFormAsync(vm);
            }
        }

        /// <summary>
        /// Displays the form for resetting a user password.
        /// </summary>
        [HttpGet]
        [Route("reset-password")]
        public async Task<ActionResult> ResetPassword(string id)
        {
            CheckPasswordAuth();

            return await ViewResetPasswordFormAsync(id);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        [HttpPost]
        [Route("reset-password")]
        public async Task<ActionResult> ResetPassword(UserPasswordEditorVM vm)
        {
            CheckPasswordAuth();

            try
            {
                await _users.ResetPasswordAsync(vm);
                return RedirectToSuccess("Пароль изменен");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return await ViewResetPasswordFormAsync(vm.Id);
            }
        }

        #region Helpers

        /// <summary>
        /// Displays the form for creating a new password-authorized user.
        /// </summary>
        private async Task<ActionResult> ViewCreateFormAsync(UserCreatorVM vm)
        {
            var pageItems = await GetPageItemsAsync(vm.PersonalPageId);

            ViewBag.Data = new UserEditorDataVM
            {
                IsSelf = false,
                UserRoleItems = ViewHelper.GetEnumSelectList(vm.Role, except: new[] { UserRole.Unvalidated }),
                PageItems = pageItems
            };

            return View("Create", vm);
        }

        /// <summary>
        /// Displays the password reset form.
        /// </summary>
        private async Task<ActionResult> ViewResetPasswordFormAsync(string id)
        {
            ViewBag.Data = await _users.GetAsync(id);
            return View("ResetPassword", new UserPasswordEditorVM { Id = id });
        }

        /// <summary>
        /// Displays the UpdateUser form.
        /// </summary>
        private async Task<ActionResult> ViewUpdateFormAsync(UserEditorVM vm)
        {
            var canCreate = await _users.CanCreatePersonalPageAsync(vm);
            var pageItems = await GetPageItemsAsync(vm.PersonalPageId);

            ViewBag.Data = new UserEditorDataVM
            {
                IsSelf = _users.IsSelf(vm.Id, User),
                UserRoleItems = ViewHelper.GetEnumSelectList(vm.Role),
                CanCreatePersonalPage = canCreate,
                PageItems = pageItems
            };

            return View("Update", vm);
        }

        /// <summary>
        /// Returns the select list for a page picker.
        /// </summary>
        private async Task<IReadOnlyList<SelectListItem>> GetPageItemsAsync(Guid? pageId)
        {
            if (pageId != null)
            {
                var page = await _db.Pages
                                    .Where(x => x.Id == pageId)
                                    .Select(x => x.Title)
                                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(page))
                    return new[] { new SelectListItem(page, pageId.Value.ToString(), true) };
            }

            return Array.Empty<SelectListItem>();
        }

        /// <summary>
        /// Checks if password authorization is enabled.
        /// </summary>
        private void CheckPasswordAuth()
        {
            if (!_config.GetStaticConfig().Auth.AllowPasswordAuth)
                throw new OperationException("Авторизация по паролю запрещена настройками.");
        }

        #endregion
    }
}
