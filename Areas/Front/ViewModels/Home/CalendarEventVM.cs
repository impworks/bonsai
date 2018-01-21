using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Home
{
    /// <summary>
    /// Information about a calendar event.
    /// </summary>
    public class CalendarEventVM
    {
        /// <summary>
        /// Number of the day.
        /// </summary>
        public int? Day { get; set; }

        /// <summary>
        /// Title of the event.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Links to related pages.
        /// </summary>
        public IReadOnlyList<PageTitleExtendedVM> RelatedPages { get; set; }
    }
}
