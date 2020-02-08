using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Page;

namespace Bonsai.Areas.Front.ViewModels.Calendar
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
        /// Event type.
        /// </summary>
        public CalendarEventType Type { get; set; }

        /// <summary>
        /// Link to the main page of the relation.
        /// </summary>
        public PageTitleExtendedVM RelatedPage { get; set; }

        /// <summary>
        /// Links to other pages (if available).
        /// </summary>
        public IReadOnlyList<PageTitleExtendedVM> OtherPages { get; set; }
    }
}
