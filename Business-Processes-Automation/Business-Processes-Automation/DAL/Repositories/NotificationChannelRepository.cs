using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class NotificationChannelRepository : INotificationChannelRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationChannelRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<NotificationChannel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<NotificationChannel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NotificationChannel> CreateAsync(NotificationChannel entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NotificationChannel> UpdateAsync(NotificationChannel entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
