using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IPostPublicationRepository
{
    Task<PostPublication?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostPublication>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PostPublication> CreateAsync(PostPublication entity, CancellationToken cancellationToken = default);
    Task<PostPublication> UpdateAsync(PostPublication entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
