using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.UI.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Business_Processes_Automation.UI.Controllers;

[Authorize]
[ApiController]
[Route("api/masters/me/schedule")]
public class MasterScheduleController : ControllerBase
{
    private readonly IMasterScheduleSettingsService _scheduleSettingsService;
    private readonly IMasterScheduleApiService _scheduleApiService;

    public MasterScheduleController(
        IMasterScheduleSettingsService scheduleSettingsService,
        IMasterScheduleApiService scheduleApiService)
    {
        _scheduleSettingsService = scheduleSettingsService;
        _scheduleApiService = scheduleApiService;
    }

    [HttpGet("settings")]
    public async Task<ActionResult<ScheduleSettingsResponseDTO>> GetSettings(
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var settings = await _scheduleSettingsService.GetBookingSettingsAsync(masterId, cancellationToken);

        if (settings is null)
        {
            return NotFound(new { message = ApiScheduleMessages.SettingsNotFound });
        }

        return Ok(settings);
    }

    [HttpPatch("settings")]
    public async Task<ActionResult<ScheduleSettingsResponseDTO>> UpdateSettings(
        [FromBody] UpdateScheduleSettingsRequestDTO dto,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _scheduleSettingsService.UpdateBookingSettingsAsync(
            masterId,
            dto,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        var updated = await _scheduleSettingsService.GetBookingSettingsAsync(masterId, cancellationToken);

        if (updated is null)
        {
            return NotFound(new { message = ApiScheduleMessages.SettingsNotFound });
        }

        return Ok(updated);
    }

    [HttpGet("working-hours")]
    public async Task<ActionResult<IReadOnlyList<WorkingHoursDayResponseDTO>>> GetWorkingHours(
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var workingHours = await _scheduleSettingsService.GetWorkingHoursWeekAsync(masterId, cancellationToken);
        return Ok(workingHours);
    }

    [HttpPut("working-hours")]
    public async Task<ActionResult<IReadOnlyList<WorkingHoursDayResponseDTO>>> ReplaceWorkingHours(
        [FromBody] List<WorkingHoursDayRequestDTO> days,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        if (days is null || days.Count == 0)
        {
            return BadRequest(new { message = ScheduleSettingsMessages.WorkingHoursMustContainSevenDays });
        }

        var result = await _scheduleSettingsService.ReplaceWorkingHoursAsync(
            masterId,
            days,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        var updated = await _scheduleSettingsService.GetWorkingHoursWeekAsync(masterId, cancellationToken);
        return Ok(updated);
    }

    [HttpPut("working-hours/{day}")]
    public async Task<ActionResult<WorkingHoursDayResponseDTO>> UpdateWorkingHoursDay(
        Weekday day,
        [FromBody] UpdateWorkingHoursDayRequestDTO dto,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var result = await _scheduleSettingsService.UpdateWorkingHoursDayAsync(
            masterId,
            day,
            dto,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        var week = await _scheduleSettingsService.GetWorkingHoursWeekAsync(masterId, cancellationToken);
        var updatedDay = week.First(x => x.DayOfWeek == day);

        return Ok(updatedDay);
    }

    [HttpDelete("working-hours/{day}")]
    public async Task<IActionResult> DeleteWorkingHoursDay(
        Weekday day,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var result = await _scheduleSettingsService.DeleteWorkingHoursForDayAsync(
            masterId,
            day,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    [HttpGet("time-offs")]
    public async Task<ActionResult<IReadOnlyList<TimeOffResponseDTO>>> GetTimeOffs(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        if (to < from)
        {
            return BadRequest(new { message = ScheduleSettingsMessages.TimeOffDateRangeInvalid });
        }

        try
        {
            var timeOffs = await _scheduleSettingsService.GetTimeOffsAsync(
                masterId,
                from,
                to,
                cancellationToken);

            return Ok(timeOffs);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("time-offs")]
    public async Task<ActionResult<TimeOffResponseDTO>> CreateTimeOff(
        [FromBody] CreateTimeOffRequestDTO dto,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (result, created) = await _scheduleSettingsService.CreateTimeOffFromLocalAsync(
            masterId,
            dto,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        if (created is null)
        {
            return BadRequest(new { message = ApiScheduleMessages.TimeOffNotFound });
        }

        return CreatedAtAction(nameof(GetTimeOffs), new { from = DateOnly.FromDateTime(created.StartLocal), to = DateOnly.FromDateTime(created.EndLocal) }, created);
    }

    [HttpDelete("time-offs/{id:int}")]
    public async Task<IActionResult> DeleteTimeOff(int id, CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var result = await _scheduleSettingsService.DeleteTimeOffForMasterAsync(
            masterId,
            id,
            cancellationToken);

        if (!result.Success)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<CalendarResponseDTO>> GetCalendar(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] bool includeCancelled = false,
        [FromQuery] AppointmentStatus? status = null,
        [FromQuery] int? serviceId = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        if (to < from)
        {
            return BadRequest(new { message = ScheduleSettingsMessages.TimeOffDateRangeInvalid });
        }

        try
        {
            var calendar = await _scheduleApiService.GetCalendarAsync(
                masterId,
                from,
                to,
                includeCancelled,
                status,
                serviceId,
                cancellationToken);

            return Ok(calendar);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("free-slots")]
    public async Task<ActionResult<FreeSlotsResponseDTO>> GetFreeSlots(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int serviceId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        if (to < from)
        {
            return BadRequest(new { message = ScheduleSettingsMessages.TimeOffDateRangeInvalid });
        }

        if (serviceId <= 0)
        {
            return BadRequest(new { message = ApiScheduleMessages.ServiceIdRequired });
        }

        try
        {
            var freeSlots = await _scheduleApiService.GetFreeSlotsAsync(
                masterId,
                from,
                to,
                serviceId,
                cancellationToken);

            return Ok(freeSlots);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("appointments/{id:int}")]
    public async Task<ActionResult<AppointmentDetailsResponseDTO>> GetAppointment(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var appointment = await _scheduleApiService.GetAppointmentDetailsAsync(
            masterId,
            id,
            cancellationToken);

        if (appointment is null)
        {
            return NotFound(new { message = ApiScheduleMessages.AppointmentNotFound });
        }

        return Ok(appointment);
    }

    private bool TryGetMasterId(out int masterId)
    {
        masterId = default;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return !string.IsNullOrWhiteSpace(userIdString) && int.TryParse(userIdString, out masterId);
    }
}
