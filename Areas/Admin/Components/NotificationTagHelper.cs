using Bonsai.Areas.Admin.Logic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// Displays a notification if the user has not discarded it.
    /// </summary>
    [HtmlTargetElement("div", Attributes = "notification-id")]
    public class NotificationTagHelper: TagHelper
    {
        public NotificationTagHelper(NotificationsService notifications)
        {
            _notifications = notifications;
        }

        private readonly NotificationsService _notifications;
        
        /// <summary>
        /// ID of the notification.
        /// </summary>
        [HtmlAttributeName("notification-id")]
        public string NotificationId { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if(!_notifications.IsShown(NotificationId))
            { 
                output.SuppressOutput();
                return;
            }

            output.PreContent.AppendHtml($@"<button type=""button"" class=""close cmd-dismiss-notification"" data-notification-id=""{NotificationId}"">&times;</button>");
        }
    }
}
