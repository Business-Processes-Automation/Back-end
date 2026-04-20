using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IClientNotificationPreferenceRepository
{
    Task<ClientNotificationPreference?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientNotificationPreference>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientNotificationPreference> CreateAsync(ClientNotificationPreference entity, CancellationToken cancellationToken = default);
    Task<ClientNotificationPreference> UpdateAsync(ClientNotificationPreference entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
