using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdForMasterAsync(
        int id,
        int masterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Expense>> GetByMasterInRangeAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<Expense> CreateAsync(Expense entity, CancellationToken cancellationToken = default);

    Task<Expense> UpdateAsync(Expense entity, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(
        int id,
        int masterId,
        CancellationToken cancellationToken = default);
}
