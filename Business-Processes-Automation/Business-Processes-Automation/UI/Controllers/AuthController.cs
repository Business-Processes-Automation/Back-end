using Business_Processes_Automation.BLL.DTOs;
using Business_Processes_Automation.BLL.DTOs.Master;
using Business_Processes_Automation.BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Business_Processes_Automation.UI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(
            [FromBody] RegisterRequestDTO dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.RegisterAsync(
                    dto,
                    cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(
            [FromBody] LoginRequestDTO dto,
            CancellationToken cancellationToken)
        {
            var user = await _authService.LoginAsync(
                dto,
                cancellationToken);

            if (user is null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return Ok(user);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new
            {
                message = "Logged out successfully."
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthResponseDTO>> GetCurrentUser(
            CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdString);

            var user = await _authService.GetCurrentUserAsync(
                userId,
                cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
    }

}
