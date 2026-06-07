using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class ScheduledNotificationRepository : IScheduledNotificationRepository
{
    private readonly AppDbContext _dbContext;

    public ScheduledNotificationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ScheduledNotification> CreateAsync(
        ScheduledNotification entity,
        CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.ScheduledNotifications.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyList<ScheduledNotification>> GetDuePendingAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;

        return await _dbContext.ScheduledNotifications
            .Where(x => x.Status == ScheduledNotificationStatus.Pending
                        && x.ScheduledAtUtc <= nowUtc)
            .OrderBy(x => x.ScheduledAtUtc)
            .ThenBy(x => x.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsPendingAsync(
        int appointmentId,
        ScheduledNotificationKind kind,
        CancellationToken cancellationToken = default) =>
        _dbContext.ScheduledNotifications.AnyAsync(
            x => x.AppointmentId == appointmentId
                 && x.Kind == kind
                 && x.Status == ScheduledNotificationStatus.Pending,
            cancellationToken);

    public async Task MarkSentAsync(
        int id,
        DateTime sentAtUtc,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ScheduledNotifications
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.Status = ScheduledNotificationStatus.Sent;
        entity.SentAtUtc = sentAtUtc;
        entity.LastError = null;
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        int id,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ScheduledNotifications
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.Status = ScheduledNotificationStatus.Failed;
        entity.LastError = TruncateError(errorMessage);
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CancelPendingByAppointmentIdAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.ScheduledNotifications
            .Where(x => x.AppointmentId == appointmentId
                        && x.Status == ScheduledNotificationStatus.Pending)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return 0;
        }

        var nowUtc = DateTime.UtcNow;
        foreach (var entity in pending)
        {
            entity.Status = ScheduledNotificationStatus.Cancelled;
            entity.UpdatedAt = nowUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return pending.Count;
    }

    private static string TruncateError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return "Unknown error";
        }

        var trimmed = errorMessage.Trim();
        return trimmed.Length <= 2000 ? trimmed : trimmed[..2000];
    }
}
