namespace Business_Processes_Automation.Telegram.Configuration;

public class TelegramBotOptions
{
    public const string SectionName = "Telegram";

    public string BotToken { get; set; } = string.Empty;

    public bool UsePolling { get; set; } = true;

    public string? BotUsername { get; set; }
}
