using Business_Processes_Automation.BLL.DTOs.Master;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.BLL.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IMasterAppointmentSettingRepository _appointmentSettingRepository;
    private readonly PasswordHasher<Master> _passwordHasher = new();

    public AuthService(
        AppDbContext dbContext,
        IMasterAppointmentSettingRepository appointmentSettingRepository)
    {
        _dbContext = dbContext;
        _appointmentSettingRepository = appointmentSettingRepository;
    }

    public async Task<AuthResponseDTO> RegisterAsync(
        RegisterRequestDTO dto,
        CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim();
        var username = dto.Username.Trim();

        var existingEmail = await _dbContext.Masters
            .AnyAsync(x => !x.IsDeleted && x.Email == email, cancellationToken);

        if (existingEmail)
        {
            throw new InvalidOperationException(AuthMessages.EmailAlreadyExists);
        }

        var existingUsername = await _dbContext.Masters
            .AnyAsync(x => !x.IsDeleted && x.Username == username, cancellationToken);

        if (existingUsername)
        {
            throw new InvalidOperationException(AuthMessages.UsernameAlreadyTaken);
        }

        var master = new Master
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Username = username,
            Email = email,
            PhoneNumber = dto.PhoneNumber.Trim(),
            TimeZone = string.IsNullOrWhiteSpace(dto.TimeZone) ? "UTC" : dto.TimeZone.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        master.PasswordHash = _passwordHasher.HashPassword(master, dto.Password);

        await _dbContext.Masters.AddAsync(master, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _appointmentSettingRepository.CreateAsync(
            new MasterAppointmentSetting
            {
                MasterId = master.Id,
                MinBookingNoticeMinutes = 60,
                MaxBookingDaysAhead = 30,
                CancellationPolicyHours = 24,
                BufferBetweenClientsMinutes = 10,
                FreeSlotIntervalMinutes = 15,
                MaxRescheduleCount = 1
            },
            cancellationToken);

        return MapMasterToAuthResponse(master);
    }

    public async Task<AuthResponseDTO?> LoginAsync(
        LoginRequestDTO dto,
        CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim();

        var master = await _dbContext.Masters
            .FirstOrDefaultAsync(
                x => !x.IsDeleted && x.IsActive && x.Email == email,
                cancellationToken);

        if (master is null || string.IsNullOrEmpty(master.PasswordHash))
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

        return MapMasterToAuthResponse(master);
    }

    public async Task<AuthResponseDTO?> GetCurrentUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var master = await _dbContext.Masters
            .FirstOrDefaultAsync(
                x => x.Id == userId && !x.IsDeleted && x.IsActive,
                cancellationToken);

        return master is null ? null : MapMasterToAuthResponse(master);
    }

    private static AuthResponseDTO MapMasterToAuthResponse(Master master) =>
        new()
        {
            Id = master.Id,
            Username = master.Username,
            Email = master.Email,
            FirstName = master.FirstName,
            LastName = master.LastName,
            PhoneNumber = master.PhoneNumber,
            TimeZone = master.TimeZone
        };
}
