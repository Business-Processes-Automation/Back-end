namespace Business_Processes_Automation.BLL.DTOs.Master;

public class TelegramLinkStatusResponseDTO
{
    public bool IsLinked { get; set; }

    public string? BotStartParameter { get; set; }

    public string? ClientLinkUrl { get; set; }
}
