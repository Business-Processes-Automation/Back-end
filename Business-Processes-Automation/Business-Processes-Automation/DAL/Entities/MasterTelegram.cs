namespace Business_Processes_Automation.DAL.Entities;

public class MasterTelegram : BaseEntity
{
    public int MasterId { get; set; }
    public long TelegramUserId { get; set; }
    public string TelegramUsername { get; set; } = string.Empty;
    public string BotStartParameter { get; set; } = string.Empty;

    // Navigation property
    public Master Master { get; set; } = null!;
}
