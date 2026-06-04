using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken = default);

    Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Client> CreateAsync(Client entity, CancellationToken cancellationToken = default);
    Task<Client> UpdateAsync(Client entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
