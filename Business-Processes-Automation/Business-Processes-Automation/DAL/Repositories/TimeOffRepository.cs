using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class TimeOffRepository : ITimeOffRepository
{
    private readonly AppDbContext _dbContext;

    public TimeOffRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TimeOff?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.TimeOffs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TimeOff>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.TimeOffs
            .OrderBy(x => x.MasterId)
            .ThenBy(x => x.StartDateTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TimeOff>> GetByMasterIdInRangeAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default) =>
        await _dbContext.TimeOffs
            .Where(x => x.MasterId == masterId
                        && x.StartDateTime < toUtc
                        && x.EndDateTime > fromUtc)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync(cancellationToken);

    public Task<bool> HasOverlapAsync(
        int masterId,
        DateTime startUtc,
        DateTime endUtc,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TimeOffs
            .Where(x => x.MasterId == masterId
                        && x.StartDateTime < endUtc
                        && x.EndDateTime > startUtc);

        if (excludeId is { } id)
        {
            query = query.Where(x => x.Id != id);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<TimeOff> CreateAsync(TimeOff entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.TimeOffs.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TimeOff> UpdateAsync(TimeOff entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.TimeOffs.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.TimeOffs
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
