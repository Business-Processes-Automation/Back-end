using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public interface IMasterService
{
    Task<MasterTelegram?> GetByBotStartParameterAsync(
        string botStartParameter,
        CancellationToken cancellationToken = default);

    Task<Master?> GetByIdAsync(int masterId, CancellationToken cancellationToken = default);

    string GetDisplayName(Master master);
}
