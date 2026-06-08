using Business_Processes_Automation.BLL.DTOs.Finance;

namespace Business_Processes_Automation.BLL.Interfaces;

public interface IMasterFinanceService
{
    Task<RevenueReportDTO> GetRevenueAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int? serviceId = null,
        CancellationToken cancellationToken = default);

    Task<ExpenseReportDTO> GetExpensesReportAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<FinanceSummaryDTO> GetSummaryAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseCategoryResponseDTO>> GetExpenseCategoriesAsync(
        CancellationToken cancellationToken = default);

    Task<ExpenseResponseDTO?> GetExpenseByIdAsync(
        int masterId,
        int expenseId,
        CancellationToken cancellationToken = default);

    Task<ExpenseResponseDTO> CreateExpenseAsync(
        int masterId,
        CreateExpenseRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<ExpenseResponseDTO?> UpdateExpenseAsync(
        int masterId,
        int expenseId,
        UpdateExpenseRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteExpenseAsync(
        int masterId,
        int expenseId,
        CancellationToken cancellationToken = default);
}
