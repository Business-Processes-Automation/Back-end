using Business_Processes_Automation.BLL.DTOs.Finance;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class MasterFinanceService : IMasterFinanceService
{
    private static readonly AppointmentStatus[] CompletedStatuses = [AppointmentStatus.Completed];

    private readonly IMasterRepository _masterRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _expenseCategoryRepository;

    public MasterFinanceService(
        IMasterRepository masterRepository,
        IAppointmentRepository appointmentRepository,
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository expenseCategoryRepository)
    {
        _masterRepository = masterRepository;
        _appointmentRepository = appointmentRepository;
        _expenseRepository = expenseRepository;
        _expenseCategoryRepository = expenseCategoryRepository;
    }

    public async Task<RevenueReportDTO> GetRevenueAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int? serviceId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);

        var master = await GetMasterAsync(masterId, cancellationToken);
        var timeZone = MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(from, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(to, timeZone);

        var appointments = await _appointmentRepository.GetByMasterIdInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            CompletedStatuses,
            serviceId,
            cancellationToken);

        var items = appointments
            .Select(appointment => new RevenueLineItemDTO
            {
                AppointmentId = appointment.Id,
                ServiceId = appointment.ServiceId,
                ServiceName = appointment.Service.ServiceName,
                ClientName = appointment.Client.ClientName,
                VisitDate = MasterTimeZoneHelper.ToLocalDate(appointment.StartDateTime, timeZone),
                StartLocal = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone),
                Amount = appointment.PriceAtBooking
            })
            .ToList();

        var byService = appointments
            .GroupBy(x => new { x.ServiceId, x.Service.ServiceName })
            .Select(group => new RevenueByServiceDTO
            {
                ServiceId = group.Key.ServiceId,
                ServiceName = group.Key.ServiceName,
                Count = group.Count(),
                Total = group.Sum(x => x.PriceAtBooking)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.ServiceName)
            .ToList();

        return new RevenueReportDTO
        {
            From = from,
            To = to,
            TimeZone = master.TimeZone,
            TotalRevenue = items.Sum(x => x.Amount),
            CompletedAppointmentsCount = items.Count,
            Items = items,
            ByService = byService
        };
    }

    public async Task<ExpenseReportDTO> GetExpensesReportAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);

        var expenses = await _expenseRepository.GetByMasterInRangeAsync(
            masterId,
            from,
            to,
            categoryId,
            cancellationToken);

        var items = expenses.Select(MapExpense).ToList();

        var byCategory = expenses
            .GroupBy(x => new { x.ExpenseCategoryId, x.ExpenseCategory.NameOfExpense })
            .Select(group => new ExpensesByCategoryDTO
            {
                CategoryId = group.Key.ExpenseCategoryId,
                CategoryName = group.Key.NameOfExpense,
                Count = group.Count(),
                Total = group.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.CategoryName)
            .ToList();

        return new ExpenseReportDTO
        {
            From = from,
            To = to,
            TotalExpenses = items.Sum(x => x.Amount),
            ExpensesCount = items.Count,
            Items = items,
            ByCategory = byCategory
        };
    }

    public async Task<FinanceSummaryDTO> GetSummaryAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var revenueTask = GetRevenueAsync(masterId, from, to, serviceId: null, cancellationToken);
        var expensesTask = GetExpensesReportAsync(masterId, from, to, categoryId: null, cancellationToken);

        await Task.WhenAll(revenueTask, expensesTask);

        var revenue = await revenueTask;
        var expenses = await expensesTask;

        return new FinanceSummaryDTO
        {
            From = from,
            To = to,
            Revenue = revenue.TotalRevenue,
            Expenses = expenses.TotalExpenses,
            Profit = revenue.TotalRevenue - expenses.TotalExpenses,
            CompletedAppointmentsCount = revenue.CompletedAppointmentsCount,
            ExpensesCount = expenses.ExpensesCount
        };
    }

    public async Task<IReadOnlyList<ExpenseCategoryResponseDTO>> GetExpenseCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _expenseCategoryRepository.GetAllActiveAsync(cancellationToken);

        return categories
            .Select(x => new ExpenseCategoryResponseDTO
            {
                Id = x.Id,
                Name = x.NameOfExpense,
                Description = x.Description
            })
            .ToList();
    }

    public async Task<ExpenseResponseDTO?> GetExpenseByIdAsync(
        int masterId,
        int expenseId,
        CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdForMasterAsync(expenseId, masterId, cancellationToken);
        return expense is null ? null : MapExpense(expense);
    }

    public async Task<ExpenseResponseDTO> CreateExpenseAsync(
        int masterId,
        CreateExpenseRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(request.ExpenseCategoryId, cancellationToken);
        await EnsureExpenseDateNotInFutureAsync(masterId, request.DateOfExpense, cancellationToken);

        var expense = await _expenseRepository.CreateAsync(
            new Expense
            {
                MasterId = masterId,
                ExpenseCategoryId = request.ExpenseCategoryId,
                Amount = request.Amount,
                DateOfExpense = ToStorageDate(request.DateOfExpense),
                Description = NormalizeDescription(request.Description)
            },
            cancellationToken);

        var created = await _expenseRepository.GetByIdForMasterAsync(expense.Id, masterId, cancellationToken);
        return MapExpense(created!);
    }

    public async Task<ExpenseResponseDTO?> UpdateExpenseAsync(
        int masterId,
        int expenseId,
        UpdateExpenseRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdForMasterAsync(expenseId, masterId, cancellationToken);
        if (expense is null)
        {
            return null;
        }

        if (request.ExpenseCategoryId is not null)
        {
            await EnsureCategoryExistsAsync(request.ExpenseCategoryId.Value, cancellationToken);
            expense.ExpenseCategoryId = request.ExpenseCategoryId.Value;
        }

        if (request.Amount is not null)
        {
            expense.Amount = request.Amount.Value;
        }

        if (request.DateOfExpense is not null)
        {
            await EnsureExpenseDateNotInFutureAsync(masterId, request.DateOfExpense.Value, cancellationToken);
            expense.DateOfExpense = ToStorageDate(request.DateOfExpense.Value);
        }

        if (request.Description is not null)
        {
            expense.Description = NormalizeDescription(request.Description);
        }

        await _expenseRepository.UpdateAsync(expense, cancellationToken);

        var updated = await _expenseRepository.GetByIdForMasterAsync(expenseId, masterId, cancellationToken);
        return MapExpense(updated!);
    }

    public Task<bool> DeleteExpenseAsync(
        int masterId,
        int expenseId,
        CancellationToken cancellationToken = default) =>
        _expenseRepository.SoftDeleteAsync(expenseId, masterId, cancellationToken);

    private async Task<Master> GetMasterAsync(int masterId, CancellationToken cancellationToken) =>
        await _masterRepository.GetByIdAsync(masterId, cancellationToken)
        ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

    private static void ValidateDateRange(DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            throw new InvalidOperationException(MasterAppointmentMessages.InvalidDateRange);
        }
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _expenseCategoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            throw new InvalidOperationException(MasterFinanceMessages.ExpenseCategoryNotFound);
        }
    }

    private async Task EnsureExpenseDateNotInFutureAsync(
        int masterId,
        DateOnly dateOfExpense,
        CancellationToken cancellationToken)
    {
        var master = await GetMasterAsync(masterId, cancellationToken);
        var timeZone = MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);

        if (dateOfExpense > today)
        {
            throw new InvalidOperationException(MasterFinanceMessages.ExpenseDateInFuture);
        }
    }

    private static DateTime ToStorageDate(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static ExpenseResponseDTO MapExpense(Expense expense) =>
        new()
        {
            Id = expense.Id,
            ExpenseCategoryId = expense.ExpenseCategoryId,
            CategoryName = expense.ExpenseCategory.NameOfExpense,
            Amount = expense.Amount,
            DateOfExpense = DateOnly.FromDateTime(expense.DateOfExpense),
            Description = expense.Description,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
}
