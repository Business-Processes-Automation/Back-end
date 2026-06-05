using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Enums;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Results;

namespace Business_Processes_Automation.BLL.Services;

public class MasterScheduleViewService : IMasterScheduleViewService
{
    private readonly IMasterAvailabilityService _availabilityService;
    private readonly IMasterScheduleSettingsService _scheduleSettingsService;
    private readonly IAppointmentRepository _appointmentRepository;

    public MasterScheduleViewService(
        IMasterAvailabilityService availabilityService,
        IMasterScheduleSettingsService scheduleSettingsService,
        IAppointmentRepository appointmentRepository)
    {
        _availabilityService = availabilityService;
        _scheduleSettingsService = scheduleSettingsService;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<MasterScheduleViewResult> BuildViewAsync(
        int masterId,
        ScheduleViewPeriod period,
        CancellationToken cancellationToken = default)
    {
        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);
        var (rangeStart, rangeEnd) = SchedulePeriodHelper.GetDateRange(period, today);

        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(rangeStart, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(rangeEnd, timeZone);

        var workingHours = await _scheduleSettingsService.GetWorkingHoursAsync(masterId, cancellationToken);
        var timeOffs = await _scheduleSettingsService.GetTimeOffsInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            cancellationToken);

        var appointments = await _availabilityService.GetAppointmentsAsync(
            masterId,
            fromUtc,
            toUtc,
            MasterScheduleTextFormatter.GetViewStatuses(),
            cancellationToken);

        return MasterScheduleTextFormatter.FormatPeriod(
            period,
            rangeStart,
            rangeEnd,
            timeZone,
            workingHours,
            timeOffs,
            appointments);
    }

    public async Task<string?> BuildAppointmentDetailsAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdForMasterAsync(
            appointmentId,
            masterId,
            cancellationToken);

        if (appointment is null)
        {
            return null;
        }

        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        return MasterScheduleTextFormatter.FormatAppointmentDetails(appointment, timeZone);
    }

}
