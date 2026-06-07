namespace Business_Processes_Automation.BLL.Interfaces;

public interface INotificationService
{
    Task NotifyBookingConfirmedAsync(int appointmentId, CancellationToken cancellationToken = default);

    Task ScheduleRemindersAsync(int appointmentId, CancellationToken cancellationToken = default);

    Task NotifyBookingCancelledAsync(int appointmentId, CancellationToken cancellationToken = default);

    /// <param name="previousStartUtc">Час початку до переносу (UTC), для тексту «було/стало».</param>
    Task NotifyBookingRescheduledAsync(
        int appointmentId,
        DateTime? previousStartUtc = null,
        CancellationToken cancellationToken = default);
}
