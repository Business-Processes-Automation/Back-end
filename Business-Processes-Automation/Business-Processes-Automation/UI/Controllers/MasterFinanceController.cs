using Business_Processes_Automation.BLL.DTOs.Finance;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.UI.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Business_Processes_Automation.UI.Controllers;

[Authorize]
[ApiController]
[Route("api/masters/me/finance")]
public class MasterFinanceController : ControllerBase
{
    private readonly IMasterFinanceService _financeService;

    public MasterFinanceController(IMasterFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportDTO>> GetRevenue(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? serviceId,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        try
        {
            var report = await _financeService.GetRevenueAsync(
                masterId,
                from,
                to,
                serviceId,
                cancellationToken);

            return Ok(report);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("expenses")]
    public async Task<ActionResult<ExpenseReportDTO>> GetExpensesReport(
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

            return Ok(report);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<FinanceSummaryDTO>> GetSummary(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out var masterId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        try
        {
            var summary = await _financeService.GetSummaryAsync(masterId, from, to, cancellationToken);
            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("expense-categories")]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryResponseDTO>>> GetExpenseCategories(
        CancellationToken cancellationToken)
    {
        if (!TryGetMasterId(out _))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var categories = await _financeService.GetExpenseCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    private bool TryGetMasterId(out int masterId)
    {
        masterId = default;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return !string.IsNullOrWhiteSpace(userIdString) && int.TryParse(userIdString, out masterId);
    }
}
