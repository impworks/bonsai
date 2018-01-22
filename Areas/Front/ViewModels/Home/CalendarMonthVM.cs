using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Home
{
    /// <summary>
    /// The month of events.
    /// </summary>
    public class CalendarMonthVM
    {
        /// <summary>
        /// Current year.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Current month's number (1-based).
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Name of the current month.
        /// </summary>
        public string MonthName { get; set; }

        /// <summary>
        /// List of calendar days / weeks.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<CalendarDayVM>> Weeks { get; set; }

        /// <summary>
        /// Events without a certain date.
        /// </summary>
        public IReadOnlyList<CalendarEventVM> FuzzyEvents { get; set; }
    }
}
