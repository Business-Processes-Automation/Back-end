using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public class MasterService(IMasterTelegramRepository masterTelegramRepository) : IMasterService
{
    public async Task<MasterTelegram?> GetByBotStartParameterAsync(
        string botStartParameter,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(botStartParameter))
        {
            return null;
        }

        var normalized = botStartParameter.Trim().ToLowerInvariant();

        var masterTelegram = await masterTelegramRepository.GetByBotStartParameterAsync(
            normalized,
            cancellationToken);

        if (masterTelegram?.Master is not { IsActive: true })
        {
            return null;
        }

        return masterTelegram;
    }

    public string GetDisplayName(Master master) =>
        $"{master.FirstName} {master.LastName}".Trim();
}
