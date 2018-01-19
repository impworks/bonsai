using System;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Home;
using Bonsai.Data;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// The service that calculates events for the frontpage's calendar.
    /// </summary>
    public class CalendarPresenterService
    {
        public CalendarPresenterService(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        /// <summary>
        /// Returns the events to display for the current month.
        /// </summary>
        public async Task<CalendarMonthVM> GetEventsForMonthAsync(int year, int month)
        {
            throw new NotImplementedException();
        }
    }
}
