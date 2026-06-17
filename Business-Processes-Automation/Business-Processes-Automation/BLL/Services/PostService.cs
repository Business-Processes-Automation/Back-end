using Business_Processes_Automation.BLL.DTOs.Post;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IMasterRepository _masterRepository;

    public PostService(IPostRepository postRepository, IMasterRepository masterRepository)
    {
        _postRepository = postRepository;
        _masterRepository = masterRepository;
    }

    public async Task<PostResponseDTO> CreateAsync(
        CreatePostRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var master = await _masterRepository.GetByIdAsync(request.MasterId, cancellationToken);
        if (master is null)
        {
            throw new InvalidOperationException(PostManagementMessages.MasterNotFound);
        }

        var post = new Post
        {
            MasterId = request.MasterId,
            Text = request.Text.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            Status = request.Status,
            ScheduledAt = request.Status == PostStatus.Scheduled ? request.ScheduledAt : null,
            PublishedAt = request.Status == PostStatus.Published ? DateTime.UtcNow : null
        };

        var created = await _postRepository.CreateAsync(post, cancellationToken);
        return MapToDto(created);
    }

    public async Task<PostResponseDTO?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken);
        return post is null ? null : MapToDto(post);
    }

    public async Task<IReadOnlyList<PostResponseDTO>> GetAllByMasterAsync(
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var posts = await _postRepository.GetByMasterIdAsync(masterId, cancellationToken);
        return posts.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _postRepository.SoftDeleteAsync(id, cancellationToken);
    }

    private static PostResponseDTO MapToDto(Post post) =>
        new()
        {
            Id = post.Id,
            MasterId = post.MasterId,
            Text = post.Text,
            ImageUrl = post.ImageUrl,
            Status = post.Status,
            ScheduledAt = post.ScheduledAt,
            PublishedAt = post.PublishedAt,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
}
