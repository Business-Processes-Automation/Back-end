using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IScheduledNotificationRepository
{
    Task<ScheduledNotification> CreateAsync(
        ScheduledNotification entity,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScheduledNotification>> GetDuePendingAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsPendingAsync(
        int appointmentId,
        ScheduledNotificationKind kind,
        CancellationToken cancellationToken = default);

    Task MarkSentAsync(
        int id,
        DateTime sentAtUtc,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        int id,
        string errorMessage,
        CancellationToken cancellationToken = default);

    Task<int> CancelPendingByAppointmentIdAsync(
        int appointmentId,
        CancellationToken cancellationToken = default);
}
