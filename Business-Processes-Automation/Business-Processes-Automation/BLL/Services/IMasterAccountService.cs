namespace Business_Processes_Automation.BLL.Services;

public interface IMasterAccountService
{
    Task<MasterAccountResult> LogoutAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default);

    Task<MasterAccountResult> DeleteAccountAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default);
}

public sealed class MasterAccountResult
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public static MasterAccountResult Ok() => new() { Success = true };

    public static MasterAccountResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
