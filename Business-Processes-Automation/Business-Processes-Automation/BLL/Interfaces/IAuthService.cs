using Business_Processes_Automation.BLL.DTOs.Master;

namespace Business_Processes_Automation.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(
            RegisterRequestDTO dto,
            CancellationToken cancellationToken = default);

        Task<AuthResponseDTO?> LoginAsync(
            LoginRequestDTO dto,
            CancellationToken cancellationToken = default);

        Task<AuthResponseDTO?> GetCurrentUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }

}
