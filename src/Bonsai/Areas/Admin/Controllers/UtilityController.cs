using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers;

/// <summary>
/// Controller for various utility methods.
/// </summary>
[Route("admin/util")]
public class UtilityController(NotificationsService notificationsSvc) : AdminControllerBase
{
    /// <summary>
    /// Hides the notification.
    /// </summary>
    [Route("hideNotification")]
    public async Task<ActionResult> HideNotification(string id)
    {
        notificationsSvc.Hide(id);
        return Ok();
    }
}