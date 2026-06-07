using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;

namespace Business_Processes_Automation.BLL.Hosting;

public class NotificationDispatchHostedService : BackgroundService
{
    private const int PollIntervalSeconds = 60;
    private const int BatchSize = 50;
    private const string SendFailedMessage = "Failed to send Telegram message.";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationDispatchHostedService> _logger;

    public NotificationDispatchHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationDispatchHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Notification dispatch worker started (interval {IntervalSeconds}s, batch {BatchSize}).",
            PollIntervalSeconds,
            BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchDueNotificationsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in notification dispatch worker.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Notification dispatch worker stopped.");
    }

    private async Task DispatchDueNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IScheduledNotificationRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<ITelegramNotificationSender>();

        var dueNotifications = await repository.GetDuePendingAsync(BatchSize, cancellationToken);
        if (dueNotifications.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Dispatching {Count} due notification(s).", dueNotifications.Count);

        var sentAtUtc = DateTime.UtcNow;

        foreach (var notification in dueNotifications)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var sent = await sender.TrySendAsync(
                notification.TelegramChatId,
                notification.MessageText,
                cancellationToken);

            if (sent)
            {
                await repository.MarkSentAsync(notification.Id, sentAtUtc, cancellationToken);
                _logger.LogInformation(
                    "Sent {Kind} notification {NotificationId} for appointment {AppointmentId}.",
                    notification.Kind,
                    notification.Id,
                    notification.AppointmentId);
            }
            else
            {
                await repository.MarkFailedAsync(notification.Id, SendFailedMessage, cancellationToken);
                _logger.LogWarning(
                    "Failed to send {Kind} notification {NotificationId} for appointment {AppointmentId}.",
                    notification.Kind,
                    notification.Id,
                    notification.AppointmentId);
            }
        }
    }
}
