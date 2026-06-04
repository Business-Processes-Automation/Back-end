using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class MasterTelegramRepository : IMasterTelegramRepository
{
    private readonly AppDbContext _dbContext;

    public MasterTelegramRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MasterTelegram?> GetByBotStartParameterAsync(
        string botStartParameter,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.MasterTelegrams
            .Include(x => x.Master)
            .FirstOrDefaultAsync(
                x => x.BotStartParameter == botStartParameter,
                cancellationToken);
    }

    public Task<MasterTelegram?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default) =>
        _dbContext.MasterTelegrams
            .Include(x => x.Master)
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);

    public Task<MasterTelegram?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.MasterTelegrams
            .Include(x => x.Master)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MasterTelegram>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.MasterTelegrams
            .Include(x => x.Master)
            .ToListAsync(cancellationToken);
    }

    public async Task<MasterTelegram> CreateAsync(MasterTelegram entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.MasterTelegrams.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<MasterTelegram> UpdateAsync(MasterTelegram entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.MasterTelegrams.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.MasterTelegrams
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
