using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _dbContext;

    public ClientRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Client?> GetByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken = default) =>
        _dbContext.Clients.FirstOrDefaultAsync(
            x => x.ClientTelegramId == telegramUserId && !x.IsDeleted,
            cancellationToken);

    public Task<Client?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default) =>
        _dbContext.Clients.FirstOrDefaultAsync(
            x => !x.IsDeleted && x.ClientPhone == phone,
            cancellationToken);

    public Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Clients.ToListAsync(cancellationToken);

    public async Task<Client> CreateAsync(Client entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.Clients.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Client> UpdateAsync(Client entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.Clients.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Clients.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

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
