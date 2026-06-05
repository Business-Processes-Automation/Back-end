namespace Business_Processes_Automation.BLL.DTOs.Master;

public class TelegramLinkCodeResponseDTO
{
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}
