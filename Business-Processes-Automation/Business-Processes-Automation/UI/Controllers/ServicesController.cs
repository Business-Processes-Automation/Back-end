using Business_Processes_Automation.BLL.DTOs.Service;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.UI.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Business_Processes_Automation.UI.Controllers;

[Authorize]
[ApiController]
[Route("api/masters/me/services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceManagementService _serviceManagementService;

    public ServicesController(IServiceManagementService serviceManagementService)
    {
        _serviceManagementService = serviceManagementService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceResponseDTO>>> GetAll(
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var services = await _serviceManagementService.GetByMasterIdAsync(masterId, cancellationToken);
        return Ok(services);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponseDTO>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var service = await _serviceManagementService.GetByIdAsync(masterId, id, cancellationToken);
        if (service is null)
        {
            return NotFound(new { message = ApiServicesMessages.ServiceNotFound });
        }

        return Ok(service);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponseDTO>> Create(
        [FromBody] CreateServiceRequestDTO dto,
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

        try
        {
            var created = await _serviceManagementService.CreateAsync(masterId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceResponseDTO>> Update(
        int id,
        [FromBody] UpdateServiceRequestDTO dto,
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

        try
        {
            var updated = await _serviceManagementService.UpdateAsync(masterId, id, dto, cancellationToken);
            if (updated is null)
            {
                return NotFound(new { message = ApiServicesMessages.ServiceNotFound });
            }

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        try
        {
            var deleted = await _serviceManagementService.DeleteAsync(masterId, id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { message = ApiServicesMessages.ServiceNotFound });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetMasterId(out int masterId)
    {
        masterId = default;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return !string.IsNullOrWhiteSpace(userIdString) && int.TryParse(userIdString, out masterId);
    }
}
