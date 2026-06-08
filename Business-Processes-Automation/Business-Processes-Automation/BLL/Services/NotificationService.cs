using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class NotificationService : INotificationService
{
    private const int Reminder24HoursMinutes = 24 * 60;
    private const int Reminder1HourMinutes = 60;

    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterRepository _masterRepository;
    private readonly IScheduledNotificationRepository _scheduledNotificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IAppointmentRepository appointmentRepository,
        IMasterRepository masterRepository,
        IScheduledNotificationRepository scheduledNotificationRepository,
        ILogger<NotificationService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _masterRepository = masterRepository;
        _scheduledNotificationRepository = scheduledNotificationRepository;
        _logger = logger;
    }

    public async Task NotifyBookingConfirmedAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var context = await TryLoadNotificationContextAsync(appointmentId, cancellationToken);
        if (context is null)
        {
            return;
        }

        var (appointment, timeZone, master, chatId) = context.Value;
        var messageText = NotificationMessageBuilder.Build(
            ScheduledNotificationKind.BookingConfirmed,
            appointment,
            timeZone,
            master);

        await EnqueueIfNotPendingAsync(
            appointment.Id,
            chatId,
            ScheduledNotificationKind.BookingConfirmed,
            messageText,
            DateTime.UtcNow,
            cancellationToken);
    }

    public async Task ScheduleRemindersAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var context = await TryLoadNotificationContextAsync(appointmentId, cancellationToken);
        if (context is null)
        {
            return;
        }

        var (appointment, timeZone, master, chatId) = context.Value;
        var nowUtc = DateTime.UtcNow;
        var startUtc = appointment.StartDateTime;

        var reminder24Utc = startUtc.AddMinutes(-Reminder24HoursMinutes);
        if (reminder24Utc > nowUtc)
        {
            var message24 = NotificationMessageBuilder.Build(
                ScheduledNotificationKind.Reminder24Hours,
                appointment,
                timeZone,
                master);

            await EnqueueIfNotPendingAsync(
                appointment.Id,
                chatId,
                ScheduledNotificationKind.Reminder24Hours,
                message24,
                reminder24Utc,
                cancellationToken);
        }

        var reminder1Utc = startUtc.AddMinutes(-Reminder1HourMinutes);
        if (reminder1Utc > nowUtc)
        {
            var message1 = NotificationMessageBuilder.Build(
                ScheduledNotificationKind.Reminder1Hour,
                appointment,
                timeZone,
                master);

            await EnqueueIfNotPendingAsync(
                appointment.Id,
                chatId,
                ScheduledNotificationKind.Reminder1Hour,
                message1,
                reminder1Utc,
                cancellationToken);
        }
    }

    public async Task NotifyBookingCancelledAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var context = await TryLoadNotificationContextAsync(appointmentId, cancellationToken);
        if (context is null)
        {
            return;
        }

        var (appointment, timeZone, master, chatId) = context.Value;

        await _scheduledNotificationRepository.CancelPendingByAppointmentIdAsync(
            appointment.Id,
            cancellationToken);

        var messageText = NotificationMessageBuilder.Build(
            ScheduledNotificationKind.BookingCancelled,
            appointment,
            timeZone,
            master);

        await EnqueueIfNotPendingAsync(
            appointment.Id,
            chatId,
            ScheduledNotificationKind.BookingCancelled,
            messageText,
            DateTime.UtcNow,
            cancellationToken);
    }

    public async Task NotifyBookingRescheduledAsync(
        int appointmentId,
        DateTime? previousStartUtc = null,
        CancellationToken cancellationToken = default)
    {
        var context = await TryLoadNotificationContextAsync(appointmentId, cancellationToken);
        if (context is null)
        {
            return;
        }

        var (appointment, timeZone, master, chatId) = context.Value;

        await _scheduledNotificationRepository.CancelPendingByAppointmentIdAsync(
            appointment.Id,
            cancellationToken);

        DateTime? previousStartLocal = previousStartUtc.HasValue
            ? MasterTimeZoneHelper.ToLocal(previousStartUtc.Value, timeZone)
            : null;

        var messageText = NotificationMessageBuilder.Build(
            ScheduledNotificationKind.BookingRescheduled,
            appointment,
            timeZone,
            master,
            previousStartLocal: previousStartLocal);

        await EnqueueIfNotPendingAsync(
            appointment.Id,
            chatId,
            ScheduledNotificationKind.BookingRescheduled,
            messageText,
            DateTime.UtcNow,
            cancellationToken);

        await ScheduleRemindersAsync(appointmentId, cancellationToken);
    }

    public async Task NotifyMasterNewBookingAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null || appointment.IsDeleted)
        {
            _logger.LogDebug(
                "Master notification skipped: appointment {AppointmentId} not found.",
                appointmentId);
            return;
        }

        var master = await _masterRepository.GetByIdWithTelegramAsync(
            appointment.Service.MasterId,
            cancellationToken);

        if (master?.MasterTelegram is not { TelegramUserId: var masterChatId })
        {
            _logger.LogDebug(
                "Master notification skipped: master {MasterId} has no linked Telegram.",
                appointment.Service.MasterId);
            return;
        }

        var timeZone = MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
        var messageText = NotificationMessageBuilder.Build(
            ScheduledNotificationKind.NewBookingForMaster,
            appointment,
            timeZone);

        await EnqueueIfNotPendingAsync(
            appointment.Id,
            masterChatId,
            ScheduledNotificationKind.NewBookingForMaster,
            messageText,
            DateTime.UtcNow,
            cancellationToken);
    }

    private async Task<(Appointment Appointment, TimeZoneInfo TimeZone, Master? Master, long ChatId)?>
        TryLoadNotificationContextAsync(int appointmentId, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null || appointment.IsDeleted)
        {
            _logger.LogDebug("Notification skipped: appointment {AppointmentId} not found.", appointmentId);
            return null;
        }

        if (appointment.Client.ClientTelegramId is not long chatId)
        {
            _logger.LogDebug(
                "Notification skipped: client for appointment {AppointmentId} has no Telegram id.",
                appointmentId);
            return null;
        }

        var master = await _masterRepository.GetByIdAsync(appointment.Service.MasterId, cancellationToken);
        var timeZone = MasterTimeZoneHelper.ResolveTimeZone(master?.TimeZone);

        return (appointment, timeZone, master, chatId);
    }

    private async Task EnqueueIfNotPendingAsync(
        int appointmentId,
        long telegramChatId,
        ScheduledNotificationKind kind,
        string messageText,
        DateTime scheduledAtUtc,
        CancellationToken cancellationToken)
    {
        if (await _scheduledNotificationRepository.ExistsPendingAsync(appointmentId, kind, cancellationToken))
        {
            _logger.LogDebug(
                "Notification skipped: pending {Kind} already exists for appointment {AppointmentId}.",
                kind,
                appointmentId);
            return;
        }

        await _scheduledNotificationRepository.CreateAsync(
            new ScheduledNotification
            {
                AppointmentId = appointmentId,
                Kind = kind,
                TelegramChatId = telegramChatId,
                MessageText = messageText,
                ScheduledAtUtc = scheduledAtUtc,
                Status = ScheduledNotificationStatus.Pending
            },
            cancellationToken);

        _logger.LogInformation(
            "Queued {Kind} notification for appointment {AppointmentId} at {ScheduledAtUtc:O}.",
            kind,
            appointmentId,
            scheduledAtUtc);
    }
}
