using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class MasterRepository : IMasterRepository
{
    private readonly AppDbContext _dbContext;

    public MasterRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Master?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Masters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public Task<Master?> GetByIdWithTelegramAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Masters
            .Include(x => x.MasterTelegram)
            .Include(x => x.AppointmentSetting)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public Task<Master?> GetByTelegramLinkCodeAsync(string linkCode, CancellationToken cancellationToken = default) =>
        _dbContext.Masters
            .Include(x => x.MasterTelegram)
            .Include(x => x.AppointmentSetting)
            .FirstOrDefaultAsync(
                x => !x.IsDeleted && x.TelegramLinkCode == linkCode,
                cancellationToken);

    public Task<IReadOnlyList<Master>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Master> CreateAsync(Master entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.Masters.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Master> UpdateAsync(Master entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.Masters.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
