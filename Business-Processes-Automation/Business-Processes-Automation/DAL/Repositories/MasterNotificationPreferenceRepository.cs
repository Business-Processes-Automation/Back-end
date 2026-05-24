using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class MasterNotificationPreferenceRepository : IMasterNotificationPreferenceRepository
{
    private readonly AppDbContext _dbContext;

    public MasterNotificationPreferenceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MasterNotificationPreference?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterNotificationPreference>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MasterNotificationPreference> CreateAsync(MasterNotificationPreference entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MasterNotificationPreference> UpdateAsync(MasterNotificationPreference entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
