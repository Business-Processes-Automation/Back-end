using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _dbContext;

    public ExpenseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Expense?> GetByIdForMasterAsync(
        int id,
        int masterId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Expenses
            .Include(x => x.ExpenseCategory)
            .FirstOrDefaultAsync(
                x => x.Id == id && x.MasterId == masterId && !x.IsDeleted,
                cancellationToken);

    public async Task<IReadOnlyList<Expense>> GetByMasterInRangeAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var fromDate = from.ToDateTime(TimeOnly.MinValue);
        var toDate = to.ToDateTime(TimeOnly.MinValue);

        var query = _dbContext.Expenses
            .AsNoTracking()
            .Include(x => x.ExpenseCategory)
            .Where(x => x.MasterId == masterId
                        && !x.IsDeleted
                        && x.DateOfExpense >= fromDate
                        && x.DateOfExpense <= toDate);

        if (categoryId is not null)
        {
            query = query.Where(x => x.ExpenseCategoryId == categoryId.Value);
        }

        return await query
            .OrderByDescending(x => x.DateOfExpense)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Expense> CreateAsync(Expense entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.Expenses.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Expense> UpdateAsync(Expense entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.Expenses.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(
        int id,
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Expenses
            .FirstOrDefaultAsync(
                x => x.Id == id && x.MasterId == masterId && !x.IsDeleted,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
