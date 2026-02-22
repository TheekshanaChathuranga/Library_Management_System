namespace UserIdentityService.Api.Dtos.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? MembershipId { get; set; }
    public string? LibrarianCode { get; set; }
    public string Role { get; set; } = string.Empty;
}
