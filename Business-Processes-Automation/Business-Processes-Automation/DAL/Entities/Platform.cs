namespace Business_Processes_Automation.DAL.Entities;

public class Platform : BaseEntity
{
    public string PlatformName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property
    public ICollection<SocialAccount> SocialAccounts { get; set; } = new List<SocialAccount>();
    public ICollection<PostPublication> PostPublications { get; set; } = new List<PostPublication>();
}
