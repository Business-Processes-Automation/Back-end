using Business_Processes_Automation.BLL.DTOs.Post;

namespace Business_Processes_Automation.BLL.Interfaces;

public interface IPostService
{
    Task<PostResponseDTO> CreateAsync(
        CreatePostRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<PostResponseDTO?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PostResponseDTO>> GetAllByMasterAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
