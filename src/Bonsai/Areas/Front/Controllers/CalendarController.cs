using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Code.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers;

/// <summary>
/// The controller for calendar-related views.
/// </summary>
[Area("front")]
[Route("util/cal")]
[Authorize(Policy = AuthRequirement.Name)]
public class CalendarController(CalendarPresenterService calendarSvc) : AppControllerBase
{
    /// <summary>
    /// Displays the calendar grid.
    /// </summary>
    [Route("grid")]
    public async Task<ActionResult> MonthGrid([FromQuery] int year, [FromQuery] int month)
    {
        var vm = await calendarSvc.GetMonthEventsAsync(year, month);
        return View(vm);
    }

    /// <summary>
    /// Displays the list of events for a particular day.
    /// </summary>
    [Route("list")]
    public async Task<ActionResult> DayList([FromQuery] int year, [FromQuery] int month, [FromQuery] int? day = null)
    {
        var vm = await calendarSvc.GetDayEventsAsync(year, month, day);
        return View(vm);
    }
}