using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class MasterAppointmentService : IMasterAppointmentService
{
    private static readonly AppointmentStatus[] BlockingStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled
    ];

    private static readonly AppointmentStatus[] MutableStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled
    ];

    private static readonly AppointmentStatus[] AllStatuses = Enum.GetValues<AppointmentStatus>();

    private readonly IMasterRepository _masterRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterAppointmentSettingRepository _settingsRepository;
    private readonly IWorkingHoursPerDayRepository _workingHoursRepository;
    private readonly ITimeOffRepository _timeOffRepository;
    private readonly IMasterAvailabilityService _availabilityService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MasterAppointmentService> _logger;

    public MasterAppointmentService(
        IMasterRepository masterRepository,
        IServiceRepository serviceRepository,
        IClientRepository clientRepository,
        IAppointmentRepository appointmentRepository,
        IMasterAppointmentSettingRepository settingsRepository,
        IWorkingHoursPerDayRepository workingHoursRepository,
        ITimeOffRepository timeOffRepository,
        IMasterAvailabilityService availabilityService,
        INotificationService notificationService,
        ILogger<MasterAppointmentService> logger)
    {
        _masterRepository = masterRepository;
        _serviceRepository = serviceRepository;
        _clientRepository = clientRepository;
        _appointmentRepository = appointmentRepository;
        _settingsRepository = settingsRepository;
        _workingHoursRepository = workingHoursRepository;
        _timeOffRepository = timeOffRepository;
        _availabilityService = availabilityService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<AppointmentResponseDTO?> GetByIdAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdForMasterAsync(
            appointmentId,
            masterId,
            cancellationToken);

        if (appointment is null || appointment.IsDeleted)
        {
            return null;
        }

        var timeZone = await GetMasterTimeZoneAsync(masterId, cancellationToken);
        return AppointmentMapper.MapToResponse(appointment, timeZone);
    }

    public async Task<IReadOnlyList<AppointmentResponseDTO>> ListAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        AppointmentStatus? status,
        int? serviceId,
        CancellationToken cancellationToken = default)
    {
        if (to < from)
        {
            throw new InvalidOperationException(MasterAppointmentMessages.InvalidDateRange);
        }

        var timeZone = await GetMasterTimeZoneAsync(masterId, cancellationToken);
        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(from, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(to, timeZone);

        var statuses = status.HasValue ? (IReadOnlyCollection<AppointmentStatus>)[status.Value] : AllStatuses;

        var appointments = await _appointmentRepository.GetByMasterIdInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            statuses,
            serviceId,
            cancellationToken);

        return appointments
            .Select(x => AppointmentMapper.MapToResponse(x, timeZone))
            .ToList();
    }

    public async Task<IReadOnlyList<FreeSlot>> GetFreeSlotsForServiceAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int serviceId,
        CancellationToken cancellationToken = default)
    {
        if (to < from)
        {
            throw new InvalidOperationException(MasterAppointmentMessages.InvalidDateRange);
        }

        var service = await GetServiceForMasterAsync(masterId, serviceId, cancellationToken);
        var occupiedMinutes = ServiceOccupiedTimeHelper.ResolveTotalOccupiedMinutes(service);

        return await _availabilityService.GetFreeSlotsForPeriodAsync(
            masterId,
            from,
            to,
            occupiedMinutes,
            cancellationToken);
    }

    public async Task<FreeSlotsResponseDTO> GetFreeSlotsResponseAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int serviceId,
        CancellationToken cancellationToken = default)
    {
        var master = await _masterRepository.GetByIdAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

        var service = await GetServiceForMasterAsync(masterId, serviceId, cancellationToken);
        var timeZone = MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
        var occupiedMinutes = ServiceOccupiedTimeHelper.ResolveTotalOccupiedMinutes(service);

        var slots = await _availabilityService.GetFreeSlotsForPeriodAsync(
            masterId,
            from,
            to,
            occupiedMinutes,
            cancellationToken);

        return new FreeSlotsResponseDTO
        {
            TimeZone = master.TimeZone,
            From = from,
            To = to,
            ServiceId = service.Id,
            ServiceName = service.ServiceName,
            TotalOccupiedMinutes = occupiedMinutes,
            Slots = slots
                .Select(x => new FreeSlotResponseDTO
                {
                    Date = x.LocalDate,
                    StartLocal = MasterTimeZoneHelper.ToLocal(x.StartUtc, timeZone),
                    EndLocal = MasterTimeZoneHelper.ToLocal(x.EndUtc, timeZone)
                })
                .ToList()
        };
    }

    public async Task<AppointmentOperationResult> CreateManualAsync(
        int masterId,
        CreateAppointmentRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId, masterId, cancellationToken);
        if (service is null || service.IsDeleted)
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.ServiceNotFound);
        }

        var timeZone = await GetMasterTimeZoneAsync(masterId, cancellationToken);
        var startUtc = MasterTimeZoneHelper.ToUtc(request.StartLocal, timeZone);

        var slotError = await ValidateSlotAsync(masterId, service, startUtc, excludeAppointmentId: null, cancellationToken);
        if (slotError is not null)
        {
            return AppointmentOperationResult.Fail(slotError);
        }

        var client = await GetOrCreateClientByPhoneAsync(
            request.ClientName,
            request.ClientPhone,
            cancellationToken);

        var endUtc = ServiceOccupiedTimeHelper.GetOccupiedEndUtc(startUtc, service);

        var appointment = await _appointmentRepository.CreateAsync(
            new Appointment
            {
                ClientId = client.Id,
                ServiceId = service.Id,
                StartDateTime = startUtc,
                EndDateTime = endUtc,
                Status = AppointmentStatus.Planned,
                RescheduleCount = 0,
                PriceAtBooking = service.Price,
                PrepaymentAmount = 0
            },
            cancellationToken);

        appointment = await ReloadAppointmentAsync(appointment.Id, cancellationToken);
        return AppointmentOperationResult.Ok(appointment!);
    }

    public async Task<AppointmentOperationResult> CreateFromTelegramAsync(
        int masterId,
        long telegramUserId,
        string clientDisplayName,
        int serviceId,
        FreeSlot slot,
        CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, masterId, cancellationToken);
        if (service is null || service.IsDeleted)
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.ServiceNotFound);
        }

        var slotError = await ValidateSlotAsync(
            masterId,
            service,
            slot.StartUtc,
            excludeAppointmentId: null,
            cancellationToken);

        if (slotError is not null)
        {
            return AppointmentOperationResult.Fail(slotError);
        }

        var client = await GetOrCreateTelegramClientAsync(telegramUserId, clientDisplayName, cancellationToken);
        var endUtc = ServiceOccupiedTimeHelper.GetOccupiedEndUtc(slot.StartUtc, service);

        var appointment = await _appointmentRepository.CreateAsync(
            new Appointment
            {
                ClientId = client.Id,
                ServiceId = service.Id,
                StartDateTime = slot.StartUtc,
                EndDateTime = endUtc,
                Status = AppointmentStatus.Planned,
                RescheduleCount = 0,
                PriceAtBooking = service.Price,
                PrepaymentAmount = 0
            },
            cancellationToken);

        appointment = await ReloadAppointmentAsync(appointment.Id, cancellationToken);
        await TryNotifyBookingCreatedAsync(appointment!.Id, cancellationToken);
        return AppointmentOperationResult.Ok(appointment);
    }

    public async Task<AppointmentOperationResult> UpdateAsync(
        int masterId,
        int appointmentId,
        UpdateAppointmentRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdForMasterAsync(
            appointmentId,
            masterId,
            cancellationToken);

        if (appointment is null || appointment.IsDeleted)
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.AppointmentNotFound);
        }

        if (!MutableStatuses.Contains(appointment.Status))
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.CannotModifyTerminalStatus);
        }

        if (request.Status is not null && !IsValidStatusTransition(appointment.Status, request.Status.Value))
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.InvalidStatusTransition);
        }

        if (request.ServiceId is not null && request.ServiceId.Value != appointment.ServiceId)
        {
            var newService = await _serviceRepository.GetByIdAsync(request.ServiceId.Value, masterId, cancellationToken);
            if (newService is null || newService.IsDeleted)
            {
                return AppointmentOperationResult.Fail(MasterAppointmentMessages.ServiceNotFound);
            }

            var slotError = await ValidateSlotAsync(
                masterId,
                newService,
                appointment.StartDateTime,
                appointmentId,
                cancellationToken);

            if (slotError is not null)
            {
                return AppointmentOperationResult.Fail(slotError);
            }

            appointment.ServiceId = newService.Id;
            appointment.Service = newService;
            appointment.EndDateTime = ServiceOccupiedTimeHelper.GetOccupiedEndUtc(
                appointment.StartDateTime,
                newService);
            appointment.PriceAtBooking = newService.Price;
        }

        if (request.Status is not null)
        {
            appointment.Status = request.Status.Value;
        }

        if (request.Notes is not null)
        {
            appointment.Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim();
        }

        var updated = await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        updated = await ReloadAppointmentAsync(updated.Id, cancellationToken);
        return AppointmentOperationResult.Ok(updated!);
    }

    public async Task<AppointmentOperationResult> CancelAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdForMasterAsync(
            appointmentId,
            masterId,
            cancellationToken);

        if (appointment is null || appointment.IsDeleted)
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.AppointmentNotFound);
        }

        if (!MutableStatuses.Contains(appointment.Status))
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.CancelNotAllowed);
        }

        appointment.Status = AppointmentStatus.Cancelled;
        var updated = await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        updated = await ReloadAppointmentAsync(updated.Id, cancellationToken);
        await TryNotifyBookingCancelledAsync(updated!.Id, cancellationToken);
        return AppointmentOperationResult.Ok(updated);
    }

    public async Task<AppointmentOperationResult> RescheduleAsync(
        int masterId,
        int appointmentId,
        RescheduleAppointmentRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdForMasterAsync(
            appointmentId,
            masterId,
            cancellationToken);

        if (appointment is null || appointment.IsDeleted)
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.AppointmentNotFound);
        }

        if (!MutableStatuses.Contains(appointment.Status))
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.RescheduleNotAllowed);
        }

        var settings = await _settingsRepository.GetByMasterIdAsync(masterId, cancellationToken);
        if (settings is null)
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.SettingsNotFound);
        }

        if (settings.MaxRescheduleCount == 0)
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.RescheduleNotAllowed);
        }

        if (appointment.RescheduleCount >= settings.MaxRescheduleCount)
        {
            return AppointmentOperationResult.Fail(MasterAppointmentMessages.RescheduleLimitReached);
        }

        var timeZone = await GetMasterTimeZoneAsync(masterId, cancellationToken);
        var newStartUtc = MasterTimeZoneHelper.ToUtc(request.StartLocal, timeZone);

        var slotError = await ValidateSlotAsync(
            masterId,
            appointment.Service,
            newStartUtc,
            appointmentId,
            cancellationToken);

        if (slotError is not null)
        {
            return AppointmentOperationResult.Fail(slotError);
        }

        var previousStartUtc = appointment.StartDateTime;

        appointment.StartDateTime = newStartUtc;
        appointment.EndDateTime = ServiceOccupiedTimeHelper.GetOccupiedEndUtc(newStartUtc, appointment.Service);
        appointment.Status = AppointmentStatus.Planned;
        appointment.RescheduleCount += 1;

        var updated = await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        updated = await ReloadAppointmentAsync(updated.Id, cancellationToken);
        await TryNotifyBookingRescheduledAsync(updated!.Id, previousStartUtc, cancellationToken);
        return AppointmentOperationResult.Ok(updated);
    }

    private async Task TryNotifyBookingCreatedAsync(int appointmentId, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.NotifyBookingConfirmedAsync(appointmentId, cancellationToken);
            await _notificationService.ScheduleRemindersAsync(appointmentId, cancellationToken);
            await _notificationService.NotifyMasterNewBookingAsync(appointmentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue booking notifications for appointment {AppointmentId}.",
                appointmentId);
        }
    }

    private async Task TryNotifyBookingCancelledAsync(int appointmentId, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.NotifyBookingCancelledAsync(appointmentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue cancellation notification for appointment {AppointmentId}.",
                appointmentId);
        }
    }

    private async Task TryNotifyBookingRescheduledAsync(
        int appointmentId,
        DateTime previousStartUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.NotifyBookingRescheduledAsync(
                appointmentId,
                previousStartUtc,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue reschedule notification for appointment {AppointmentId}.",
                appointmentId);
        }
    }

    private async Task<string?> ValidateSlotAsync(
        int masterId,
        Service service,
        DateTime startUtc,
        int? excludeAppointmentId,
        CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetByMasterIdAsync(masterId, cancellationToken);
        if (settings is null)
        {
            return MasterAppointmentMessages.SettingsNotFound;
        }

        var timeZone = await GetMasterTimeZoneAsync(masterId, cancellationToken);
        var endUtc = ServiceOccupiedTimeHelper.GetOccupiedEndUtc(startUtc, service);
        var bufferMinutes = settings.BufferBetweenClientsMinutes;

        if (startUtc < DateTime.UtcNow.AddMinutes(settings.MinBookingNoticeMinutes))
        {
            return MasterAppointmentMessages.MinBookingNoticeViolation;
        }

        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);
        var appointmentDate = MasterTimeZoneHelper.ToLocalDate(startUtc, timeZone);
        if (appointmentDate > today.AddDays(settings.MaxBookingDaysAhead))
        {
            return MasterAppointmentMessages.MaxBookingDaysExceeded;
        }

        var weekday = (Weekday)(int)appointmentDate.DayOfWeek;
        var workingHours = await _workingHoursRepository.GetByMasterIdAsync(masterId, cancellationToken);
        var dayHours = workingHours.FirstOrDefault(x => x.DayOfWeek == weekday && !x.IsDeleted);
        if (dayHours is null)
        {
            return MasterAppointmentMessages.NonWorkingDay;
        }

        var localStart = MasterTimeZoneHelper.ToLocal(startUtc, timeZone);
        var localEnd = MasterTimeZoneHelper.ToLocal(endUtc, timeZone);
        var startTime = TimeOnly.FromDateTime(localStart);
        var endTime = TimeOnly.FromDateTime(localEnd);

        if (DateOnly.FromDateTime(localEnd) != appointmentDate
            || startTime < dayHours.WorkStartTime
            || endTime > dayHours.WorkEndTime)
        {
            return MasterAppointmentMessages.OutsideWorkingHours;
        }

        var (dayStartUtc, dayEndUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(appointmentDate, timeZone);
        var timeOffs = await _timeOffRepository.GetByMasterIdInRangeAsync(
            masterId,
            dayStartUtc,
            dayEndUtc,
            cancellationToken);

        if (ScheduleDisplayHelper.IsFullDayOff(timeOffs, dayStartUtc, dayEndUtc))
        {
            return MasterAppointmentMessages.FullDayOff;
        }

        if (timeOffs.Any(x => x.StartDateTime < endUtc && x.EndDateTime > startUtc))
        {
            return MasterAppointmentMessages.SlotUnavailable;
        }

        var blocking = await _appointmentRepository.GetByMasterIdInRangeAsync(
            masterId,
            dayStartUtc,
            dayEndUtc,
            BlockingStatuses,
            serviceId: null,
            cancellationToken);

        foreach (var existing in blocking)
        {
            if (excludeAppointmentId.HasValue && existing.Id == excludeAppointmentId.Value)
            {
                continue;
            }

            if (ServiceOccupiedTimeHelper.OverlapsBusyBlock(startUtc, endUtc, existing, bufferMinutes))
            {
                return MasterAppointmentMessages.SlotAlreadyTaken;
            }
        }

        return null;
    }

    private static bool IsValidStatusTransition(AppointmentStatus current, AppointmentStatus next) =>
        current switch
        {
            AppointmentStatus.Planned or AppointmentStatus.Rescheduled =>
                next is AppointmentStatus.Completed
                    or AppointmentStatus.Cancelled
                    or AppointmentStatus.NoShow,
            _ => false
        };

    private async Task<Client> GetOrCreateClientByPhoneAsync(
        string clientName,
        string clientPhone,
        CancellationToken cancellationToken)
    {
        var phone = clientPhone.Trim();
        var name = clientName.Trim();
        if (name.Length > 200)
        {
            name = name[..200];
        }

        var existing = await _clientRepository.GetByPhoneAsync(phone, cancellationToken);
        if (existing is not null)
        {
            if (existing.ClientName != name)
            {
                existing.ClientName = name;
                await _clientRepository.UpdateAsync(existing, cancellationToken);
            }

            return existing;
        }

        return await _clientRepository.CreateAsync(
            new Client
            {
                ClientName = name,
                ClientPhone = phone
            },
            cancellationToken);
    }

    private async Task<Client> GetOrCreateTelegramClientAsync(
        long telegramUserId,
        string clientDisplayName,
        CancellationToken cancellationToken)
    {
        var existing = await _clientRepository.GetByTelegramIdAsync(telegramUserId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var name = string.IsNullOrWhiteSpace(clientDisplayName) ? "Клієнт" : clientDisplayName.Trim();
        if (name.Length > 200)
        {
            name = name[..200];
        }

        return await _clientRepository.CreateAsync(
            new Client
            {
                ClientName = name,
                ClientPhone = "—",
                ClientTelegramId = telegramUserId
            },
            cancellationToken);
    }

    private async Task<Appointment?> ReloadAppointmentAsync(int appointmentId, CancellationToken cancellationToken) =>
        await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);

    private async Task<Service> GetServiceForMasterAsync(
        int masterId,
        int serviceId,
        CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, masterId, cancellationToken);
        if (service is null || service.IsDeleted)
        {
            throw new InvalidOperationException(MasterAppointmentMessages.ServiceNotFound);
        }

        return service;
    }

    private async Task<TimeZoneInfo> GetMasterTimeZoneAsync(int masterId, CancellationToken cancellationToken)
    {
        var master = await _masterRepository.GetByIdAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

        return MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
    }
}
