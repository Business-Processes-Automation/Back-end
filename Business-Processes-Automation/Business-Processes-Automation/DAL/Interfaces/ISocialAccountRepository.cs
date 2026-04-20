using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface ISocialAccountRepository
{
    Task<SocialAccount?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialAccount>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SocialAccount> CreateAsync(SocialAccount entity, CancellationToken cancellationToken = default);
    Task<SocialAccount> UpdateAsync(SocialAccount entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
