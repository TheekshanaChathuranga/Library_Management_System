using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserIdentityService.Api.Constants;
using UserIdentityService.Api.Dtos.Auth;
using UserIdentityService.Api.Services.Auth;

namespace UserIdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Librarian}")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RegisterAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to register user {Email}", request.Email);
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Login failed for {Email}", request.Email);
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RefreshAsync(request.RefreshToken, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Refresh token rejected");
            return Unauthorized(new { message = "Invalid refresh token" });
        }
    }
}
