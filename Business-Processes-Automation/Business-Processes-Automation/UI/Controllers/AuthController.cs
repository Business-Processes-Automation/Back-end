using Business_Processes_Automation.BLL.DTOs.Master;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.Telegram.Configuration;
using Business_Processes_Automation.UI.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Business_Processes_Automation.UI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMasterTelegramLinkService _telegramLinkService;
    private readonly TelegramBotOptions _botOptions;

    public AuthController(
        IAuthService authService,
        IMasterTelegramLinkService telegramLinkService,
        IOptions<TelegramBotOptions> botOptions)
    {
        _authService = authService;
        _telegramLinkService = telegramLinkService;
        _botOptions = botOptions.Value;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register(
        [FromBody] RegisterRequestDTO dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _authService.RegisterAsync(dto, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login(
        [FromBody] LoginRequestDTO dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _authService.LoginAsync(dto, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { message = ApiAuthMessages.InvalidCredentials });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });

        return Ok(user);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = ApiAuthMessages.LogoutSuccess });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponseDTO>> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = ApiCommonMessages.UserNotFound });
        }

        return Ok(user);
    }

    [Authorize]
    [HttpPost("me/telegram/link-code")]
    public async Task<ActionResult<TelegramLinkCodeResponseDTO>> CreateTelegramLinkCode(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        try
        {
            var result = await _telegramLinkService.GenerateLinkCodeAsync(userId, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me/telegram")]
    public async Task<ActionResult<TelegramLinkStatusResponseDTO>> GetTelegramLinkStatus(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { message = ApiCommonMessages.Unauthorized });
        }

        try
        {
            var status = await _telegramLinkService.GetTelegramLinkStatusAsync(userId, cancellationToken);
            if (status.IsLinked && !string.IsNullOrWhiteSpace(status.BotStartParameter))
            {
                status.ClientLinkUrl = BuildClientLink(status.BotStartParameter);
            }

            return Ok(status);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userIdString) && int.TryParse(userIdString, out userId);
    }

    private string BuildClientLink(string botStartParameter)
    {
        var botUsername = _botOptions.BotUsername;
        if (string.IsNullOrWhiteSpace(botUsername))
        {
            return $"?start={botStartParameter}";
        }

        return $"https://t.me/{botUsername.TrimStart('@')}?start={botStartParameter}";
    }
}
