using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class AIContentGeneration : BaseEntity
{
    public int PostId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Prompt { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? GeneratedText { get; set; }

    [MaxLength(2048)]
    [Url]
    public string? GeneratedImageUrl { get; set; }

    public Post Post { get; set; } = null!;
}
