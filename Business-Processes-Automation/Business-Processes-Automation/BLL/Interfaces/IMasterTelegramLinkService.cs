using Business_Processes_Automation.BLL.DTOs.Master;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL.Entities;
namespace Business_Processes_Automation.BLL.Services;

public interface IMasterTelegramLinkService
{
    Task<MasterTelegram?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default);

    Task<bool> IsBotStartParameterTakenAsync(
        string botStartParameter,
        CancellationToken cancellationToken = default);

    Task<TelegramLinkCodeResponseDTO> GenerateLinkCodeAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<TelegramLinkStatusResponseDTO> GetTelegramLinkStatusAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<string?> ValidateLinkCodeAsync(
        string linkCode,
        CancellationToken cancellationToken = default);

    Task<MasterTelegramLinkResult> LinkTelegramAsync(
        string linkCode,
        long telegramUserId,
        string telegramUsername,
        string botStartParameter,
        CancellationToken cancellationToken = default);
}
