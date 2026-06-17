namespace Business_Processes_Automation.BLL.DTOs.AI
{
    public class GenerateAndSaveRequestDTO
    {
        public int PostId { get; set; }
        public string Prompt { get; set; } = string.Empty;
    }
}
