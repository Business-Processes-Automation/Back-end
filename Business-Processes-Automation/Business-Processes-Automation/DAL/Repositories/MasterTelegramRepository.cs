using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class MasterTelegramRepository : IMasterTelegramRepository
{
    private readonly AppDbContext _dbContext;

    public MasterTelegramRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MasterTelegram?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterTelegram>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MasterTelegram> CreateAsync(MasterTelegram entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MasterTelegram> UpdateAsync(MasterTelegram entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
