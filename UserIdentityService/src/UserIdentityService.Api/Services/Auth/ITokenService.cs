using System.Security.Claims;
using UserIdentityService.Api.Entities;

namespace UserIdentityService.Api.Services.Auth;

public interface ITokenService
{
    Task<(string AccessToken, DateTime ExpiresAt)> CreateAccessTokenAsync(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
    RefreshToken CreateRefreshToken(ApplicationUser user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
