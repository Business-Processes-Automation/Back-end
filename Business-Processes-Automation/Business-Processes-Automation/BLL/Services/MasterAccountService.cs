namespace Business_Processes_Automation.BLL.Services;

public class MasterAccountService : IMasterAccountService
{
    private readonly ITelegramUserSessionService _sessionService;

    public MasterAccountService(ITelegramUserSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task<MasterAccountResult> LogoutAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default)
    {
        await _sessionService.ClearSessionAsync(telegramUserId, chatId, cancellationToken);
        return MasterAccountResult.Ok();
    }
}
