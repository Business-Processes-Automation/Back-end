using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
