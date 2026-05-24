namespace Business_Processes_Automation.Telegram.Configuration;

public class TelegramBotOptions
{
    public const string SectionName = "Telegram";

    /// <summary>
    /// Token from @BotFather. Prefer User Secrets or appsettings.Development.local.json in local dev.
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// Use long polling (recommended for local development).
    /// </summary>
    public bool UsePolling { get; set; } = true;

    /// <summary>
    /// Bot username without @, e.g. service_booking_bot (for client deep links).
    /// </summary>
    public string? BotUsername { get; set; }
}
