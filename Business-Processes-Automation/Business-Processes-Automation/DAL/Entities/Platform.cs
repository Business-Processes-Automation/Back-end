using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class Platform : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string PlatformName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<SocialAccount> SocialAccounts { get; set; } = new List<SocialAccount>();
    public ICollection<PostPublication> PostPublications { get; set; } = new List<PostPublication>();
}
