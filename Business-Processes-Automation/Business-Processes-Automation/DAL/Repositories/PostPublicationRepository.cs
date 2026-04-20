using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class PostPublicationRepository : IPostPublicationRepository
{
    private readonly AppDbContext _dbContext;

    public PostPublicationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PostPublication?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<PostPublication>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PostPublication> CreateAsync(PostPublication entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PostPublication> UpdateAsync(PostPublication entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
