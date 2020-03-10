using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Menu;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// Component for displaying the side menu of the admin panel.
    /// </summary>
    public class AdminMenuComponent: ViewComponent
    {
        public AdminMenuComponent(UserManager<AppUser> userMgr, AppDbContext db)
        {
            _userMgr = userMgr;
            _db = db;
        }

        private readonly UserManager<AppUser> _userMgr;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the menu.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userMgr.GetUserAsync(HttpContext.User);
            var roles = await _userMgr.GetRolesAsync(user);

            var groups = new List<MenuGroupVM>();
            groups.Add(
                new MenuGroupVM(
                    new MenuItemVM { Title = "Главная", Icon = "home", Url = Url.Action("Index", "Dashboard", new { area = "Admin" }) },
                    new MenuItemVM { Title = "Правки", Icon = "database", Url = Url.Action("Index", "Changesets", new { area = "Admin" }) }
                )
            );

            groups.Add(
                new MenuGroupVM(
                    new MenuItemVM { Title = "Страницы", Icon = "file-text-o", Url = Url.Action("Index", "Pages", new { area = "Admin" }) },
                    new MenuItemVM { Title = "Связи", Icon = "retweet", Url = Url.Action("Index", "Relations", new { area = "Admin" }) },
                    new MenuItemVM { Title = "Медиа", Icon = "picture-o", Url = Url.Action("Index", "Media", new { area = "Admin" })}
                )
            );

            if (roles.Contains(nameof(UserRole.Admin)))
            {
                var newUsers = await _db.Users.CountAsync(x => !x.IsValidated);
                groups.Add(
                    new MenuGroupVM(
                        new MenuItemVM { Title = "Доступ", Icon = "user-circle-o", Url = Url.Action("Index", "Users", new { area = "Admin" }), NotificationsCount = newUsers },
                        new MenuItemVM { Title = "Настройки", Icon = "cog", Url = Url.Action("Index", "DynamicConfig", new { area = "Admin" }) }
                    )
                );
            }

            var url = HttpContext.Request.Path;
            foreach (var item in groups.SelectMany(x => x.Items))
                item.IsSelected = url.StartsWithSegments(item.Url);

            return View("~/Areas/Admin/Views/Components/AdminMenu.cshtml", groups);
        }
    }
}
