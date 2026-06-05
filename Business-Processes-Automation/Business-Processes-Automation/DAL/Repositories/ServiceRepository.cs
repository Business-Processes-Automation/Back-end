using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Service>> GetByMasterIdAsync(
        int masterId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Services
            .AsNoTracking()
            .Where(x => x.MasterId == masterId && !x.IsDeleted)
            .OrderBy(x => x.ServiceName)
            .ToListAsync(cancellationToken);

    public Task<Service?> GetByIdAsync(
        int id,
        int masterId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Services.FirstOrDefaultAsync(
            x => x.Id == id && x.MasterId == masterId && !x.IsDeleted,
            cancellationToken);

    public async Task<Service> CreateAsync(Service entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.Services.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Service> UpdateAsync(Service entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.Services.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(
        int id,
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Services
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
