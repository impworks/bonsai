using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Menu;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// Component for displaying the side menu of the admin panel.
    /// </summary>
    public class AdminMenuComponent: ViewComponent
    {
        public AdminMenuComponent(UserManager<AppUser> userMgr)
        {
            _userMgr = userMgr;
        }

        private readonly UserManager<AppUser> _userMgr;

        /// <summary>
        /// Displays the menu.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userMgr.GetUserAsync(HttpContext.User).ConfigureAwait(false);
            var roles = await _userMgr.GetRolesAsync(user).ConfigureAwait(false);

            var groups = new List<MenuGroupVM>();
            groups.Add(
                new MenuGroupVM(
                    new MenuItemVM { Title = "Главная", Url = Url.Action("Index", "Dashboard", new { area = "Admin" }) }
                )
            );

            groups.Add(
                new MenuGroupVM(
                    new MenuItemVM { Title = "Страницы", Url = "/#" },
                    new MenuItemVM { Title = "Медиа", Url = "/#" }
                )
            );

            if (roles.Contains(RoleNames.AdminRole))
            {
                groups.Add(
                    new MenuGroupVM(
                        new MenuItemVM { Title = "Пользователи", Url = "/#" },
                        new MenuItemVM { Title = "Настройки", Url = "/#" }
                    )
                );
            }

            var url = HttpContext.Request.Path;
            foreach (var item in groups.SelectMany(x => x.Items))
                item.IsSelected = url.StartsWithSegments(item.Url);

            return View("~/Areas/Admin/Views/Menu/Index.cshtml", groups);
        }
    }
}
