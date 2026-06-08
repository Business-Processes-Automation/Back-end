using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly AppDbContext _dbContext;

    public ExpenseCategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ExpenseCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.ExpenseCategories
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<ExpenseCategory>> GetAllActiveAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.ExpenseCategories
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.NameOfExpense)
            .ToListAsync(cancellationToken);
}
