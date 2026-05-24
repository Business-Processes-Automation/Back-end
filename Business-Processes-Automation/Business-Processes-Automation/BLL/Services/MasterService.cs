using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public class MasterService : IMasterService
{
    private readonly IMasterTelegramRepository _masterTelegramRepository;
    private readonly IMasterRepository _masterRepository;

    public MasterService(
        IMasterTelegramRepository masterTelegramRepository,
        IMasterRepository masterRepository)
    {
        _masterTelegramRepository = masterTelegramRepository;
        _masterRepository = masterRepository;
    }

    public async Task<MasterTelegram?> GetByBotStartParameterAsync(
        string botStartParameter,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(botStartParameter))
        {
            return null;
        }

        var normalized = botStartParameter.Trim().ToLowerInvariant();

        var masterTelegram = await _masterTelegramRepository.GetByBotStartParameterAsync(
            normalized,
            cancellationToken);

        if (masterTelegram?.Master is not { IsActive: true, IsDeleted: false })
        {
            return null;
        }

        return masterTelegram;
    }

    public Task<Master?> GetByIdAsync(int masterId, CancellationToken cancellationToken = default) =>
        _masterRepository.GetByIdAsync(masterId, cancellationToken);

    public string GetDisplayName(Master master) =>
        $"{master.FirstName} {master.LastName}".Trim();
}
