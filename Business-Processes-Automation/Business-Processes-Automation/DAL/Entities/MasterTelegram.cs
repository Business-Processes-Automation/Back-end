using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class MasterTelegram : BaseEntity
{
    public int MasterId { get; set; }

    public long TelegramUserId { get; set; }

    [Required]
    [MaxLength(64)]
    public string TelegramUsername { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string BotStartParameter { get; set; } = string.Empty;

    public Master Master { get; set; } = null!;
}
