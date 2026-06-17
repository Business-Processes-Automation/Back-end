using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.DTOs.Post;

public class PostResponseDTO
{
    public int Id { get; set; }

    public int MasterId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public PostStatus Status { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
