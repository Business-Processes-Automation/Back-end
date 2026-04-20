using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class NotificationTypeRepository : INotificationTypeRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationTypeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<NotificationType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<NotificationType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NotificationType> CreateAsync(NotificationType entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NotificationType> UpdateAsync(NotificationType entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
