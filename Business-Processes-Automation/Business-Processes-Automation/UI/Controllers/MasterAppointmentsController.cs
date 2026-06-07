using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.UI.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Business_Processes_Automation.UI.Controllers;

[Authorize]
[ApiController]
[Route("api/masters/me/appointments")]
public class MasterAppointmentsController : ControllerBase
{
    private readonly IMasterAppointmentService _appointmentService;

    public MasterAppointmentsController(IMasterAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponseDTO>>> List(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] int? serviceId,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        try
        {
            var appointments = await _appointmentService.ListAsync(
                masterId,
                from,
                to,
                status,
                serviceId,
                cancellationToken);

            return Ok(appointments);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentResponseDTO>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var appointment = await _appointmentService.GetByIdAsync(masterId, id, cancellationToken);
        if (appointment is null)
        {
            return NotFound(new { message = ApiAppointmentMessages.AppointmentNotFound });
        }

        return Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponseDTO>> Create(
        [FromBody] CreateAppointmentRequestDTO dto,
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

        var result = await _appointmentService.CreateManualAsync(masterId, dto, cancellationToken);
        if (!result.Success || result.Appointment is null)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        var response = await _appointmentService.GetByIdAsync(
            masterId,
            result.Appointment.Id,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Appointment.Id }, response);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<AppointmentResponseDTO>> Update(
        int id,
        [FromBody] UpdateAppointmentRequestDTO dto,
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

        var result = await _appointmentService.UpdateAsync(masterId, id, dto, cancellationToken);
        if (!result.Success)
        {
            if (result.ErrorMessage == MasterAppointmentMessages.AppointmentNotFound)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }

        var response = await _appointmentService.GetByIdAsync(masterId, id, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<AppointmentResponseDTO>> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var result = await _appointmentService.CancelAsync(masterId, id, cancellationToken);
        if (!result.Success)
        {
            if (result.ErrorMessage == MasterAppointmentMessages.AppointmentNotFound)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }

        var response = await _appointmentService.GetByIdAsync(masterId, id, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:int}/reschedule")]
    public async Task<ActionResult<AppointmentResponseDTO>> Reschedule(
        int id,
        [FromBody] RescheduleAppointmentRequestDTO dto,
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

        var result = await _appointmentService.RescheduleAsync(masterId, id, dto, cancellationToken);
        if (!result.Success)
        {
            if (result.ErrorMessage == MasterAppointmentMessages.AppointmentNotFound)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }

        var response = await _appointmentService.GetByIdAsync(masterId, id, cancellationToken);
        return Ok(response);
    }

    private bool TryGetMasterId(out int masterId)
    {
        masterId = default;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return !string.IsNullOrWhiteSpace(userIdString) && int.TryParse(userIdString, out masterId);
    }
}
