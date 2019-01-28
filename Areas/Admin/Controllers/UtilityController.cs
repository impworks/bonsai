using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for various utility methods.
    /// </summary>
    [Route("admin/util")]
    public class UtilityController: AdminControllerBase
    {
        public UtilityController(NotificationsService notifications)
        {
            _notifications = notifications;
        }

        private readonly NotificationsService _notifications;

        /// <summary>
        /// Hides the notification.
        /// </summary>
        [Route("hideNotification")]
        public async Task<ActionResult> HideNotification(string id)
        {
            _notifications.Hide(id);
            return Ok();
        }
    }
}
