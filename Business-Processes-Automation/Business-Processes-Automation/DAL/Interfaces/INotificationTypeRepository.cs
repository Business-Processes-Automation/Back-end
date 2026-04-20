using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface INotificationTypeRepository
{
    Task<NotificationType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NotificationType> CreateAsync(NotificationType entity, CancellationToken cancellationToken = default);
    Task<NotificationType> UpdateAsync(NotificationType entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
