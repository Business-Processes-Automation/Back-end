using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Enums;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class ClientBookingService : IClientBookingService
{
    private static readonly AppointmentStatus[] OverviewAppointmentStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled,
        AppointmentStatus.Completed,
        AppointmentStatus.NoShow
    ];

    private readonly IMasterAvailabilityService _availabilityService;
    private readonly IServiceRepository _serviceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterScheduleSettingsService _scheduleSettingsService;

    public ClientBookingService(
        IMasterAvailabilityService availabilityService,
        IServiceRepository serviceRepository,
        IClientRepository clientRepository,
        IAppointmentRepository appointmentRepository,
        IMasterScheduleSettingsService scheduleSettingsService)
    {
        _availabilityService = availabilityService;
        _serviceRepository = serviceRepository;
        _clientRepository = clientRepository;
        _appointmentRepository = appointmentRepository;
        _scheduleSettingsService = scheduleSettingsService;
    }

    public async Task<ClientBookingViewResult> BuildOverviewAsync(
        int masterId,
        ScheduleViewPeriod period,
        CancellationToken cancellationToken = default)
    {
        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);
        var (rangeStart, rangeEnd) = SchedulePeriodHelper.GetDateRange(period, today);

        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(rangeStart, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(rangeEnd, timeZone);

        var workingHours = await _availabilityService.GetWorkingHoursForMasterAsync(masterId, cancellationToken);
        var timeOffs = await _availabilityService.GetTimeOffsAsync(masterId, fromUtc, toUtc, cancellationToken);
        var appointments = await _availabilityService.GetAppointmentsAsync(
            masterId,
            fromUtc,
            toUtc,
            OverviewAppointmentStatuses,
            cancellationToken);

        var services = await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);

        return ClientBookingViewFormatter.FormatOverview(
            period,
            rangeStart,
            rangeEnd,
            timeZone,
            workingHours,
            timeOffs,
            appointments,
            services);
    }

    public async Task<(ClientBookingViewResult View, IReadOnlyList<FreeSlot> Slots)> BuildSlotsViewAsync(
        int masterId,
        BookingDraft draft,
        int serviceId,
        CancellationToken cancellationToken = default)
    {
        var service = (await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken))
            .FirstOrDefault(x => x.Id == serviceId);

        if (service is null)
        {
            throw new InvalidOperationException("Service was not found for this master.");
        }

        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(draft.RangeStart, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(draft.RangeEnd, timeZone);

        var workingHours = await _availabilityService.GetWorkingHoursForMasterAsync(masterId, cancellationToken);
        var timeOffs = await _availabilityService.GetTimeOffsAsync(masterId, fromUtc, toUtc, cancellationToken);
        var appointments = await _availabilityService.GetAppointmentsAsync(
            masterId,
            fromUtc,
            toUtc,
            OverviewAppointmentStatuses,
            cancellationToken);

        var freeSlots = await _availabilityService.GetFreeSlotsForPeriodAsync(
            masterId,
            draft.RangeStart,
            draft.RangeEnd,
            service.DurationInMinutes,
            cancellationToken);

        var view = ClientBookingViewFormatter.FormatWithFreeSlots(
            draft.Period,
            draft.RangeStart,
            draft.RangeEnd,
            timeZone,
            service,
            workingHours,
            timeOffs,
            appointments,
            freeSlots);

        return (view, freeSlots);
    }

    public async Task<BookingResult> CreateBookingAsync(
        int masterId,
        long telegramUserId,
        string clientDisplayName,
        int serviceId,
        FreeSlot slot,
        CancellationToken cancellationToken = default)
    {
        var service = (await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken))
            .FirstOrDefault(x => x.Id == serviceId);

        if (service is null)
        {
            return BookingResult.Fail("Послугу не знайдено.");
        }

        if (await _scheduleSettingsService.GetTimeOffsInRangeAsync(
                masterId,
                slot.StartUtc,
                slot.EndUtc,
                cancellationToken) is { Count: > 0 } overlappingTimeOffs &&
            overlappingTimeOffs.Any(x => x.StartDateTime < slot.EndUtc && x.EndDateTime > slot.StartUtc))
        {
            return BookingResult.Fail("Цей час більше недоступний.");
        }

        var blocking = await _availabilityService.GetAppointmentsAsync(
            masterId,
            slot.StartUtc,
            slot.EndUtc,
            [AppointmentStatus.Planned, AppointmentStatus.Rescheduled],
            cancellationToken);

        if (blocking.Count > 0)
        {
            return BookingResult.Fail("Цей час вже зайнятий.");
        }

        var client = await GetOrCreateClientAsync(telegramUserId, clientDisplayName, cancellationToken);

        var appointment = await _appointmentRepository.CreateAsync(
            new Appointment
            {
                ClientId = client.Id,
                ServiceId = service.Id,
                StartDateTime = slot.StartUtc,
                EndDateTime = slot.EndUtc,
                Status = AppointmentStatus.Planned,
                PriceAtBooking = service.Price,
                PrepaymentAmount = service.Prepayment
            },
            cancellationToken);

        return BookingResult.Ok(appointment);
    }

    private async Task<Client> GetOrCreateClientAsync(
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

        return await _clientRepository.CreateAsync(
            new Client
            {
                ClientName = name.Length > 200 ? name[..200] : name,
                ClientPhone = "—",
                ClientTelegramId = telegramUserId
            },
            cancellationToken);
    }
}
