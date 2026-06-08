using Business_Processes_Automation.BLL.DTOs.Finance;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.UI.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Business_Processes_Automation.UI.Controllers;

[Authorize]
[ApiController]
[Route("api/masters/me/expenses")]
public class MasterExpensesController : ControllerBase
{
    private readonly IMasterFinanceService _financeService;

    public MasterExpensesController(IMasterFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponseDTO>>> List(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        try
        {
            var report = await _financeService.GetExpensesReportAsync(
                masterId,
                from,
                to,
                categoryId,
                cancellationToken);

            return Ok(report.Items);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponseDTO>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var expense = await _financeService.GetExpenseByIdAsync(masterId, id, cancellationToken);
        if (expense is null)
        {
            return NotFound(new { message = ApiFinanceMessages.ExpenseNotFound });
        }

        return Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDTO>> Create(
        [FromBody] CreateExpenseRequestDTO dto,
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
            var created = await _financeService.CreateExpenseAsync(masterId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<ExpenseResponseDTO>> Update(
        int id,
        [FromBody] UpdateExpenseRequestDTO dto,
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
            var updated = await _financeService.UpdateExpenseAsync(masterId, id, dto, cancellationToken);
            if (updated is null)
            {
                return NotFound(new { message = ApiFinanceMessages.ExpenseNotFound });
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

        var deleted = await _financeService.DeleteExpenseAsync(masterId, id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = ApiFinanceMessages.ExpenseNotFound });
        }

        return NoContent();
    }

    private bool TryGetMasterId(out int masterId)
    {
        masterId = default;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return !string.IsNullOrWhiteSpace(userIdString) && int.TryParse(userIdString, out masterId);
    }
}
