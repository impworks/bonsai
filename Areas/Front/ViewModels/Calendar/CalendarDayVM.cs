using System.Collections.Generic;
using Bonsai.Code.Utils.Date;

namespace Bonsai.Areas.Front.ViewModels.Calendar
{
    public class CalendarDayVM
    {
        /// <summary>
        /// Day number (1-based).
        /// </summary>
        public int? Day { get; set; }

        /// <summary>
        /// Date of the day.
        /// </summary>
        public FuzzyDate Date { get; set; }

        /// <summary>
        /// Flag indicating that the day belongs to currently displayed month.
        /// Otherwise, the day is a placeholder.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// List of events in the current day.
        /// </summary>
        public IReadOnlyList<CalendarEventVM> Events { get; set; }
    }
}
