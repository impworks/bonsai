using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Relations;
using Bonsai.Areas.Front.ViewModels.Home;
using Bonsai.Code.Tools;
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
            var context = await RelationContext.LoadContextAsync(_db)
                                               .ConfigureAwait(false);

            var range = GetDisplayedRange(year, month);
            var events = GetPageEvents(year, month, context)
                .Concat(GetRelationEvents(year, month, context))
                .ToList();

            // todo

            throw new NotImplementedException();
        }

        #region Private helpers

        /// <summary>
        /// Infers page-related events for the current month.
        /// </summary>
        private IEnumerable<CalendarEventVM> GetPageEvents(int year, int month, RelationContext context)
        {
            foreach (var page in context.Pages.Values)
            {
                if (page.DeathDate is FuzzyDate death && death.Month == month && !(year < death.Year))
                {
                    var title = year == death.Year
                        ? "Дата смерти"
                        : death.Year == null
                            ? "Годовщина смерти"
                            : (year - death.Year.Value) + "-ая годовщина смерти";

                    yield return new CalendarEventVM
                    {
                        Day = death.Day,
                        Title = title,
                        RelatedPages = new [] { Map(page) }
                    };
                }

                else if (page.BirthDate is FuzzyDate birth && birth.Month == month && !(year > birth.Year))
                {
                    var title = year == birth.Year
                        ? "Дата рождения"
                        : birth.Year == null
                            ? "День рождения"
                            : $"День рождения ({year - birth.Year.Value})";

                    yield return new CalendarEventVM
                    {
                        Day = birth.Day,
                        Title = title,
                        RelatedPages = new[] { Map(page) }
                    };
                }
            }
        }

        /// <summary>
        /// Infers relation-based events for the current month.
        /// </summary>
        private IEnumerable<CalendarEventVM> GetRelationEvents(int year, int month, RelationContext context)
        {
            yield break;
        }

        /// <summary>
        /// Gets the range for displaying the events.
        /// </summary>
        private FuzzyRange GetDisplayedRange(int year, int month)
        {
            var firstMonthDay = new DateTime(year, month, 1);
            var firstWeekDay = firstMonthDay.DayOfWeek;
            var daysBeforeFirst = firstWeekDay != DayOfWeek.Sunday ? (int) firstWeekDay - 1 : 6;
            var firstDay = firstMonthDay.AddDays(-daysBeforeFirst);

            var lastMonthDay = firstMonthDay.AddMonths(1).AddDays(-1);
            var lastWeekDay = lastMonthDay.DayOfWeek;
            var daysAfterLast = 7 - (lastWeekDay != DayOfWeek.Sunday ? (int) lastWeekDay : 0);
            var lastDay = lastMonthDay.AddDays(daysAfterLast);

            return new FuzzyRange(firstDay, lastDay);
        }

        /// <summary>
        /// Maps a page excerpt to the page title.
        /// </summary>
        private PageTitleExtendedVM Map(RelationContext.PageExcerpt page)
        {
            return new PageTitleExtendedVM
            {
                Key = page.Key,
                Title = page.Title,
                MainPhotoPath = page.MainPhotoPath
            };
        }

        #endregion
    }
}

