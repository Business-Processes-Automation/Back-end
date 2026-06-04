namespace Business_Processes_Automation.BLL.Services;

public interface IClientAppointmentsService
{
    Task<string> BuildMyAppointmentsMessageAsync(
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken = default);
}
