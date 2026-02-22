namespace UserIdentityService.Api.Dtos.Roles;

public class UpdateRolePermissionsRequest
{
    public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
}
