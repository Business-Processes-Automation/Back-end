using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class WorkingHoursPerDayRepository : IWorkingHoursPerDayRepository
{
    private readonly AppDbContext _dbContext;

    public WorkingHoursPerDayRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<WorkingHoursPerDay?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.WorkingHoursPerDays
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkingHoursPerDay>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.WorkingHoursPerDays
            .OrderBy(x => x.MasterId)
            .ThenBy(x => x.DayOfWeek)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkingHoursPerDay>> GetByMasterIdAsync(
        int masterId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.WorkingHoursPerDays
            .Where(x => x.MasterId == masterId)
            .OrderBy(x => x.DayOfWeek)
            .ToListAsync(cancellationToken);

    public Task<WorkingHoursPerDay?> GetByMasterAndDayAsync(
        int masterId,
        Weekday day,
        CancellationToken cancellationToken = default) =>
        _dbContext.WorkingHoursPerDays
            .FirstOrDefaultAsync(
                x => x.MasterId == masterId && x.DayOfWeek == day,
                cancellationToken);

    public async Task<WorkingHoursPerDay> CreateAsync(
        WorkingHoursPerDay entity,
        CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.WorkingHoursPerDays.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<WorkingHoursPerDay> UpdateAsync(
        WorkingHoursPerDay entity,
        CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.WorkingHoursPerDays.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.WorkingHoursPerDays
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

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
