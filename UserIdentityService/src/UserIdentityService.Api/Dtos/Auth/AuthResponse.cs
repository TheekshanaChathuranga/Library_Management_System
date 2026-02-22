namespace UserIdentityService.Api.Dtos.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
}
