using Microsoft.AspNetCore.Identity;

namespace UserIdentityService.Api.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public string? MembershipId { get; set; }
    public string? LibrarianCode { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
