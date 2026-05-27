using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces.Repositories;

namespace Business_Processes_Automation.BLL.Services;

public class ClientAppointmentsService : IClientAppointmentsService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterAvailabilityService _availabilityService;
    private readonly IMasterService _masterService;

    public ClientAppointmentsService(
        IAppointmentRepository appointmentRepository,
        IMasterAvailabilityService availabilityService,
        IMasterService masterService)
    {
        _appointmentRepository = appointmentRepository;
        _availabilityService = availabilityService;
        _masterService = masterService;
    }

    public async Task<string> BuildMyAppointmentsMessageAsync(
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var master = await _masterService.GetByIdAsync(masterId, cancellationToken);
        if (master is null)
        {
            return "Майстра не знайдено.";
        }

        var appointments = await _appointmentRepository.GetByClientTelegramIdForMasterAsync(
            telegramUserId,
            masterId,
            cancellationToken);

        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        return ClientAppointmentsTextFormatter.FormatMyAppointments(
            appointments,
            _masterService.GetDisplayName(master),
            master.PhoneNumber,
            master.TimeZone,
            timeZone);
    }
}
