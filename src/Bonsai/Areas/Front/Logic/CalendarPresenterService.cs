using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Calendar;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Impworks.Utils.Strings;

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
        public async Task<CalendarMonthVM> GetMonthEventsAsync(int year, int month)
        {
            var context = await RelationContext.LoadContextAsync(_db);

            var range = GetDisplayedRange(year, month);
            var events = GetPageEvents(year, month, context)
                         .Concat(GetRelationEvents(year, month, context))
                         .ToList();

            return new CalendarMonthVM
            {
                Month = month,
                Year = year,
                Title = new FuzzyDate(year, month, null).ReadableDate.Capitalize(),
                Weeks = GetMonthGrid(range.from, range.to, month, events),
                FuzzyEvents = events.Where(x => x.Day == null).ToList()
            };
        }

        /// <summary>
        /// Returns the events for a particular day (or fuzzy events for the month).
        /// </summary>
        public async Task<CalendarDayVM> GetDayEventsAsync(int year, int month, int? day)
        {
            var context = await RelationContext.LoadContextAsync(_db);
            var events = GetPageEvents(year, month, context)
                         .Concat(GetRelationEvents(year, month, context))
                         .Where(x => x.Day == day)
                         .ToList();

            return new CalendarDayVM
            {
                IsActive = true,
                Day = day,
                Date = new FuzzyDate(year, month, day),
                Events = events
            };
        }

        #region Private helpers

        /// <summary>
        /// Infers page-related events for the current month.
        /// </summary>
        private IEnumerable<CalendarEventVM> GetPageEvents(int year, int month, RelationContext context)
        {
            var maxDate = new FuzzyDate(new DateTime(year, month, 1).AddMonths(1).AddSeconds(-1));

            foreach (var page in context.Pages.Values)
            {
                if (page.BirthDate is FuzzyDate birth)
                {
                    var showBirth = birth.Month == month
                                    && (birth.Year == null || birth.Year <= year)
                                    && (page.DeathDate == null || page.DeathDate >= maxDate);

                    if (showBirth)
                    {
                        var title = (year == birth.Year && !birth.IsDecade)
                            ? "Дата рождения"
                            : (birth.Year == null || birth.IsDecade)
                                ? "День рождения"
                                : $"День рождения ({year - birth.Year.Value})";

                        yield return new CalendarEventVM
                        {
                            Day = birth.Day,
                            Title = title,
                            Type = CalendarEventType.Birth,
                            RelatedPage = Map(page)
                        };
                    }
                }

                if (page.DeathDate is FuzzyDate death)
                {
                    var showDeath = death.Month == month
                                    && (death.Year == null || death.Year <= year);

                    if (showDeath)
                    {
                        var title = (year == death.Year && !death.IsDecade)
                            ? "Дата смерти"
                            : (death.Year == null || death.IsDecade)
                                ? "Годовщина смерти"
                                : (year - death.Year.Value) + "-ая годовщина смерти";

                        yield return new CalendarEventVM
                        {
                            Day = death.Day,
                            Title = title,
                            Type = CalendarEventType.Death,
                            RelatedPage = Map(page)
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Infers relation-based events for the current month.
        /// </summary>
        private IEnumerable<CalendarEventVM> GetRelationEvents(int year, int month, RelationContext context)
        {
            var visited = new HashSet<string>();
            var maxDate = new FuzzyDate(CreateDate(year, month).AddMonths(1).AddSeconds(-1));

            foreach (var rel in context.Relations.SelectMany(x => x.Value))
            {
                if (!(rel.Duration is FuzzyRange duration) || !(duration.RangeStart is FuzzyDate start) || start.Month != month)
                    continue;

                if (duration.RangeEnd is FuzzyDate end && end <= maxDate)
                    continue;

                var hash = string.Concat(rel.SourceId.ToString(), rel.DestinationId.ToString(), duration.ToString());
                if (visited.Contains(hash))
                    continue;

                var inverseHash = string.Concat(rel.DestinationId.ToString(), rel.SourceId.ToString(), duration.ToString());
                visited.Add(hash);
                visited.Add(inverseHash);

                var title = (year == start.Year && !start.IsDecade)
                    ? "День свадьбы"
                    : (start.Year == null || start.IsDecade)
                        ? "Годовщина"
                        : (year - start.Year.Value) + "-ая годовщина";

                yield return new CalendarEventVM
                {
                    Day = start.Day,
                    Title = title,
                    Type = CalendarEventType.Wedding,
                    RelatedPage = rel.EventId == null
                        ? new PageTitleExtendedVM { Title = "Свадьба", MainPhotoPath = "~/assets/img/unknown-event.svg" }
                        : Map(context.Pages[rel.EventId.Value]),
                    OtherPages = new []
                    {
                        Map(context.Pages[rel.SourceId]),
                        Map(context.Pages[rel.DestinationId])
                    }
                };
            }
        }

        /// <summary>
        /// Gets the range for displaying the events.
        /// </summary>
        private (DateTime from, DateTime to) GetDisplayedRange(int year, int month)
        {
            var firstMonthDay = CreateDate(year, month);
            var firstWeekDay = firstMonthDay.DayOfWeek;
            var daysBeforeFirst = firstWeekDay != DayOfWeek.Sunday ? (int) firstWeekDay - 1 : 6;
            var firstDay = firstMonthDay.AddDays(-daysBeforeFirst);

            var lastMonthDay = firstMonthDay.AddMonths(1).AddDays(-1);
            var lastWeekDay = lastMonthDay.DayOfWeek;
            var daysAfterLast = lastWeekDay != DayOfWeek.Sunday ? 7 - (int) lastWeekDay : 0;
            var lastDay = lastMonthDay.AddDays(daysAfterLast);

            return (firstDay, lastDay);
        }

        /// <summary>
        /// Returns a proper DateTime, falling back to current month.
        /// </summary>
        private DateTime CreateDate(int year, int month)
        {
            if(month < 1 || month > 12)
            {
                var now = DateTime.Now;
                return new DateTime(now.Year, now.Month, 1);
            }

            return new DateTime(year, month, 1);
        }

        /// <summary>
        /// Gets the grid of days for current range.
        /// </summary>
        private IReadOnlyList<IReadOnlyList<CalendarDayVM>> GetMonthGrid(DateTime from, DateTime to, int month, IEnumerable<CalendarEventVM> events)
        {
            var curr = from;
            var weeks = new List<List<CalendarDayVM>>();
            var cache = events.Where(x => x.Day != null)
                              .GroupBy(x => x.Day.Value)
                              .ToDictionary(x => x.Key, x => x.ToList());

            while (curr < to)
            {
                var week = new List<CalendarDayVM>();
                for (var i = 0; i < 7; i++)
                {
                    var day = new CalendarDayVM { Day = curr.Day };
                    if (curr.Month == month)
                    {
                        day.IsActive = true;

                        if (cache.TryGetValue(curr.Day, out var dayEvents))
                            day.Events = dayEvents;
                    }

                    week.Add(day);
                    curr = curr.AddDays(1);
                }

                weeks.Add(week);
            }

            return weeks;
        }

        /// <summary>
        /// Maps a page excerpt to the page title.
        /// </summary>
        private PageTitleExtendedVM Map(RelationContext.PageExcerpt page)
        {
            return new PageTitleExtendedVM
            {
                Id = page.Id,
                Key = page.Key,
                Title = page.Title,
                Type = page.Type,
                MainPhotoPath = MediaPresenterService.GetSizedMediaPath(page.MainPhotoPath, MediaSize.Small)
            };
        }

        #endregion
    }
}

