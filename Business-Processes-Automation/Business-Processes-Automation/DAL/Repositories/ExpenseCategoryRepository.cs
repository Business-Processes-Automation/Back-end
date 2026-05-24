using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly AppDbContext _dbContext;

    public ExpenseCategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ExpenseCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ExpenseCategory> CreateAsync(ExpenseCategory entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ExpenseCategory> UpdateAsync(ExpenseCategory entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
