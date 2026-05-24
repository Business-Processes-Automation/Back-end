using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class SocialAccount : BaseEntity
{
    public int MasterId { get; set; }
    public int PlatformId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string AccessToken { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public Master Master { get; set; } = null!;
    public Platform Platform { get; set; } = null!;
}
