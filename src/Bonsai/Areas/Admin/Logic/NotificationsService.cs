using Microsoft.AspNetCore.Http;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// A service to keep track of user-dismissable notifications.
    /// </summary>
    public class NotificationsService
    {
        public NotificationsService(IHttpContextAccessor httpAccessor)
        {
            _httpAccessor = httpAccessor;
        }

        private readonly IHttpContextAccessor _httpAccessor;

        private HttpRequest Request => _httpAccessor.HttpContext.Request;
        private HttpResponse Response => _httpAccessor.HttpContext.Response;

        /// <summary>
        /// Prefix for all notification cookies.
        /// </summary>
        private static readonly string PREFIX = "Notifications.";

        /// <summary>
        /// Unique ID for the notification urging the user to see guidelines before editing a page.
        /// </summary>
        public static readonly string NOTE_USER_GUIDELINES = "UserGuidelines";

        /// <summary>
        /// Unique ID for the notification about password auth profiles.
        /// </summary>
        public static readonly string NOTE_PASSWORD_AUTH = "PasswordAuth";

        /// <summary>
        /// Notification ID: user attempted to create a new page, but a draft of a different type was already present.
        /// </summary>
        public static readonly string NOTE_PAGETYPE_RESET_FROM_DRAFT = "PageTypeResetFromDraft";

        /// <summary>
        /// Checks if the notification must be shown.
        /// </summary>
        public bool IsShown(string id)
        {
            return !Request.Cookies.ContainsKey(PREFIX + id);
        }

        /// <summary>
        /// Dismisses the notification by setting a cookie.
        /// </summary>
        public void Hide(string id)
        {
            Response.Cookies.Append(PREFIX + id, "true", new CookieOptions { IsEssential = true });
        }

        /// <summary>
        /// Clears the dismissed state.
        /// </summary>
        public void Show(string id)
        {
            _httpAccessor.HttpContext.Response.Cookies.Delete(PREFIX + id, new CookieOptions { IsEssential = true });
        }
    }
}
