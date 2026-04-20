using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class SocialAccountRepository : ISocialAccountRepository
{
    private readonly AppDbContext _dbContext;

    public SocialAccountRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SocialAccount?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<SocialAccount>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SocialAccount> CreateAsync(SocialAccount entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SocialAccount> UpdateAsync(SocialAccount entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
