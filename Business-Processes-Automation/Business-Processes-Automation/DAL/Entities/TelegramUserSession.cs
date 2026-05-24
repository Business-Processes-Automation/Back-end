using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class TelegramUserSession : BaseEntity
{
    public long TelegramUserId { get; set; }
    public long ChatId { get; set; }
    public int? MasterId { get; set; }
    public TelegramUserRole Role { get; set; } = TelegramUserRole.Client;
    public ConversationStep CurrentStep { get; set; } = ConversationStep.Idle;

    /// <summary>JSON draft for multi-step flows (e.g. master adding a service).</summary>
    public string? DraftJson { get; set; }

    public Master? Master { get; set; }
}
