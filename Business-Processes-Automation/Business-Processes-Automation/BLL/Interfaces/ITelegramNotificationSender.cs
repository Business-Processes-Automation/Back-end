namespace Business_Processes_Automation.BLL.Interfaces;

public interface ITelegramNotificationSender
{
    Task<bool> TrySendAsync(long chatId, string text, CancellationToken cancellationToken = default);
}
