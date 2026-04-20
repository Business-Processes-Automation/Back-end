namespace Business_Processes_Automation.DAL.Entities;

public class SocialAccount : BaseEntity
{
    public int MasterId { get; set; }
    public int PlatformId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public Master Master { get; set; } = null!;
    public Platform Platform { get; set; } = null!;
}
