namespace UserIdentityService.Api.Dtos.Users;

public class UserSummary
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}
