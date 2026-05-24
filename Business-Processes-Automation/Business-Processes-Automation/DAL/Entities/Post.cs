using System.ComponentModel.DataAnnotations;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class Post : BaseEntity
{
    public int MasterId { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Text { get; set; } = string.Empty;

    [MaxLength(2048)]
    [Url]
    public string? ImageUrl { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;

    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public Master Master { get; set; } = null!;
    public ICollection<AIContentGeneration> AiContentGenerations { get; set; } = new List<AIContentGeneration>();
    public ICollection<PostPublication> PostPublications { get; set; } = new List<PostPublication>();
}
