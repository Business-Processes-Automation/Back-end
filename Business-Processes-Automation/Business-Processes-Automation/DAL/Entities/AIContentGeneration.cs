namespace Business_Processes_Automation.DAL.Entities;

public class AIContentGeneration : BaseEntity
{
    public string Prompt { get; set; } = string.Empty;
    public string? GeneratedText { get; set; }
    public string? GeneratedImageUrl { get; set; }

    // Navigation property
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
}
