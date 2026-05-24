using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class PostPublication : BaseEntity
{
    public int PostId { get; set; }
    public int PlatformId { get; set; }

    public PostPublicationStatus Status { get; set; } = PostPublicationStatus.Pending;

    public DateTime? PublishedAt { get; set; }

    public Post Post { get; set; } = null!;
    public Platform Platform { get; set; } = null!;
}
