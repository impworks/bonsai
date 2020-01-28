using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Components
{
    /// <summary>
    /// The component for displaying the user bar.
    /// </summary>
    public class UserHeaderComponent: ViewComponent
    {
        public UserHeaderComponent(AuthService auth)
        {
            _auth = auth;
        }

        private readonly AuthService _auth;

        /// <summary>
        /// Displays the user info bar in the header.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _auth.GetCurrentUserAsync(HttpContext.User);
            ViewBag.ReturnUrl = Request.Path.ToString();
            return View("~/Areas/Front/Views/Components/UserHeader.cshtml", user);
        }
    }
}
