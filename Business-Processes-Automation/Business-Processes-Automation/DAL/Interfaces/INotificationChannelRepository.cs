using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface INotificationChannelRepository
{
    Task<NotificationChannel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationChannel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NotificationChannel> CreateAsync(NotificationChannel entity, CancellationToken cancellationToken = default);
    Task<NotificationChannel> UpdateAsync(NotificationChannel entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
