namespace Business_Processes_Automation.BLL.Interfaces
{
    public interface IAIContentService
    {
        Task<string> GeneratePostTextAsync(
        string prompt,
        CancellationToken cancellationToken = default);

        Task<string> GenerateAndSaveAsync(
        int postId,
        string prompt,
        CancellationToken cancellationToken = default);

    }
}
