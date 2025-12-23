using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Data.Models;
using ModelContextProtocol.Server;

namespace Bonsai.Areas.Mcp.Logic.Tools;

/// <summary>
/// MCP tools for accessing calendar events.
/// </summary>
[McpServerToolType]
public class CalendarTools(
    CalendarPresenterService calendarService,
    McpToolAuthorizationService authService)
{
    /// <summary>
    /// Gets events for a month.
    /// </summary>
    [McpServerTool(Name = "get_month_events")]
    [Description("Get calendar events for a specific month. Returns birthdays, deaths, anniversaries, weddings, and other events.")]
    public async Task<MonthEventsResult> GetMonthEvents(
        [Description("Year (e.g., 2024)")] int year,
        [Description("Month (1-12)")] int month)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var result = await calendarService.GetMonthEventsAsync(year, month);

        var events = new List<CalendarEvent>();

        foreach (var week in result.Weeks)
            foreach (var day in week.Where(d => d.IsActive && d.Events?.Any() == true))
                foreach (var evt in day.Events)
                    events.Add(MapEvent(evt, day.Day));

        // Add fuzzy events (events without specific day)
        foreach (var evt in result.FuzzyEvents ?? [])
            events.Add(MapEvent(evt, null));

        return new MonthEventsResult
        {
            Year = year,
            Month = month,
            Title = result.Title,
            Events = events.OrderBy(e => e.Day ?? 0).ThenBy(e => e.Type).ToList()
        };
    }

    /// <summary>
    /// Gets events for a specific day.
    /// </summary>
    [McpServerTool(Name = "get_day_events")]
    [Description("Get calendar events for a specific day. Returns all events that occurred on that date.")]
    public async Task<DayEventsResult> GetDayEvents(
        [Description("Year (e.g., 2024)")] int year,
        [Description("Month (1-12)")] int month,
        [Description("Day (1-31, or null for events without specific day)")] int? day = null)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var result = await calendarService.GetDayEventsAsync(year, month, day);

        var events = result.Events?
            .Select(e => MapEvent(e, day))
            .ToList() ?? [];

        return new DayEventsResult
        {
            Year = year,
            Month = month,
            Day = day,
            DateDisplay = result.Date.ReadableDate,
            Events = events
        };
    }

    /// <summary>
    /// Gets today's events.
    /// </summary>
    [McpServerTool(Name = "get_today_events")]
    [Description("Get calendar events for today.")]
    public async Task<DayEventsResult> GetTodayEvents()
    {
        await authService.RequireRoleAsync(UserRole.User);

        var today = DateTime.Today;
        return await GetDayEvents(today.Year, today.Month, today.Day);
    }

    /// <summary>
    /// Gets upcoming events for the next N days.
    /// </summary>
    [McpServerTool(Name = "get_upcoming_events")]
    [Description("Get calendar events for the next N days, including today.")]
    public async Task<UpcomingEventsResult> GetUpcomingEvents(
        [Description("Number of days to look ahead (default: 7, max: 31)")] int days = 7)
    {
        await authService.RequireRoleAsync(UserRole.User);

        days = Math.Min(days, 31);
        var allEvents = new List<UpcomingEvent>();
        var today = DateTime.Today;

        for (var i = 0; i < days; i++)
        {
            var date = today.AddDays(i);
            var dayResult = await calendarService.GetDayEventsAsync(date.Year, date.Month, date.Day);

            if (dayResult.Events?.Any() != true)
                continue;

            foreach (var evt in dayResult.Events)
            {
                allEvents.Add(new UpcomingEvent
                {
                    Date = date,
                    DaysFromToday = i,
                    Event = MapEvent(evt, date.Day)
                });
            }
        }

        return new UpcomingEventsResult
        {
            FromDate = today,
            ToDate = today.AddDays(days - 1),
            Events = allEvents
        };
    }

    private static CalendarEvent MapEvent(Bonsai.Areas.Front.ViewModels.Calendar.CalendarEventVM evt, int? day)
    {
        return new CalendarEvent
        {
            Day = day,
            Type = evt.Type.ToString(),
            Title = evt.Title,
            RelatedPage = evt.RelatedPage != null ? new CalendarPageInfo
            {
                Id = evt.RelatedPage.Id,
                Key = evt.RelatedPage.Key,
                Title = evt.RelatedPage.Title,
                Type = evt.RelatedPage.Type.ToString(),
                PhotoPath = evt.RelatedPage.MainPhotoPath
            } : null,
            OtherPages = evt.OtherPages?.Select(p => new CalendarPageInfo
            {
                Id = p.Id,
                Key = p.Key,
                Title = p.Title,
                Type = p.Type.ToString(),
                PhotoPath = p.MainPhotoPath
            }).ToList() ?? []
        };
    }
}

#region Result Types

public class MonthEventsResult
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Title { get; set; }
    public List<CalendarEvent> Events { get; set; }
}

public class DayEventsResult
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int? Day { get; set; }
    public string DateDisplay { get; set; }
    public List<CalendarEvent> Events { get; set; }
}

public class UpcomingEventsResult
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<UpcomingEvent> Events { get; set; }
}

public class UpcomingEvent
{
    public DateTime Date { get; set; }
    public int DaysFromToday { get; set; }
    public CalendarEvent Event { get; set; }
}

public class CalendarEvent
{
    public int? Day { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public CalendarPageInfo RelatedPage { get; set; }
    public List<CalendarPageInfo> OtherPages { get; set; }
}

public class CalendarPageInfo
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string PhotoPath { get; set; }
}

#endregion
