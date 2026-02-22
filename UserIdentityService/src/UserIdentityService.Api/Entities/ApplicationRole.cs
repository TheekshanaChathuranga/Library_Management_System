using Microsoft.AspNetCore.Identity;

namespace UserIdentityService.Api.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
