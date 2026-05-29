using Business_Processes_Automation.BLL.DTOs.Master;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.DAL;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;

        private readonly PasswordHasher<Master> _passwordHasher = new();

        public AuthService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AuthResponseDTO> RegisterAsync(
            RegisterRequestDTO dto,
            CancellationToken cancellationToken = default)
        {
            var existingEmail = await _dbContext.Masters
                .AnyAsync(
                    x => x.Email == dto.Email,
                    cancellationToken);

            if (existingEmail)
            {
                throw new Exception("User with this email already exists.");
            }

            var existingUsername = await _dbContext.Masters
                .AnyAsync(
                    x => x.Username == dto.Username,
                    cancellationToken);

            if (existingUsername)
            {
                throw new Exception("Username is already taken.");
            }

            var master = new Master
            {
                FirstName = dto.FirstName,

                LastName = dto.LastName,

                Username = dto.Username,

                Email = dto.Email,

                PhoneNumber = dto.PhoneNumber,

                TimeZone = dto.TimeZone,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = DateTime.UtcNow,

                IsActive = true
            };

            master.PasswordHash = _passwordHasher.HashPassword(
                master,
                dto.Password);

            await _dbContext.Masters.AddAsync(
                master,
                cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AuthResponseDTO
            {
                Id = master.Id,

                Username = master.Username,

                Email = master.Email
            };
        }

        public async Task<AuthResponseDTO?> LoginAsync(
            LoginRequestDTO dto,
            CancellationToken cancellationToken = default)
        {
            var master = await _dbContext.Masters
                .FirstOrDefaultAsync(
                    x => x.Email == dto.Email,
                    cancellationToken);

            if (master is null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(
                master,
                master.PasswordHash,
                dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return new AuthResponseDTO
            {
                Id = master.Id,

                Username = master.Username,

                Email = master.Email
            };
        }

        public async Task<AuthResponseDTO?> GetCurrentUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var master = await _dbContext.Masters
                .FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

            if (master is null)
            {
                return null;
            }

            return new AuthResponseDTO
            {
                Id = master.Id,

                Username = master.Username,

                Email = master.Email
            };
        }
    }

}
