using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class ClientNotificationPreferenceRepository : IClientNotificationPreferenceRepository
{
    private readonly AppDbContext _dbContext;

    public ClientNotificationPreferenceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ClientNotificationPreference?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ClientNotificationPreference>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClientNotificationPreference> CreateAsync(ClientNotificationPreference entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClientNotificationPreference> UpdateAsync(ClientNotificationPreference entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
