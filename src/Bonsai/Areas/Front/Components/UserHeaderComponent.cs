using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Components;

/// <summary>
/// The component for displaying the user bar.
/// </summary>
public class UserHeaderComponent(AuthService auth) : ViewComponent
{
    /// <summary>
    /// Displays the user info bar in the header.
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await auth.GetCurrentUserAsync(HttpContext.User);
        ViewBag.ReturnUrl = Request.Path.ToString();
        return View("~/Areas/Front/Views/Components/UserHeader.cshtml", user);
    }
}