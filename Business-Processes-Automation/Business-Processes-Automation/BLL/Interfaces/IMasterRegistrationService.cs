using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public interface IMasterRegistrationService
{
    Task<MasterTelegram?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default);

    Task<bool> IsBotStartParameterTakenAsync(
        string botStartParameter,
        CancellationToken cancellationToken = default);

    Task<MasterRegistrationResult> RegisterAsync(
        long telegramUserId,
        MasterRegistrationDraft draft,
        CancellationToken cancellationToken = default);
}

public sealed class MasterRegistrationResult
{
    public bool Success { get; init; }

    public Master? Master { get; init; }

    public MasterTelegram? MasterTelegram { get; init; }

    public string? ErrorMessage { get; init; }

    public static MasterRegistrationResult Ok(Master master, MasterTelegram masterTelegram) =>
        new() { Success = true, Master = master, MasterTelegram = masterTelegram };

    public static MasterRegistrationResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
