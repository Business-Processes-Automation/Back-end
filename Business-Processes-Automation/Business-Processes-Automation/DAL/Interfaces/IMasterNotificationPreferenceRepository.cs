using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IMasterNotificationPreferenceRepository
{
    Task<MasterNotificationPreference?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterNotificationPreference>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MasterNotificationPreference> CreateAsync(MasterNotificationPreference entity, CancellationToken cancellationToken = default);
    Task<MasterNotificationPreference> UpdateAsync(MasterNotificationPreference entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
