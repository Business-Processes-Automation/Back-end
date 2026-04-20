using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Post> CreateAsync(Post entity, CancellationToken cancellationToken = default);
    Task<Post> UpdateAsync(Post entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
