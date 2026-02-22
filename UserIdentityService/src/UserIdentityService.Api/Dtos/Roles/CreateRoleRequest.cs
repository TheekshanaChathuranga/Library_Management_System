namespace UserIdentityService.Api.Dtos.Roles;

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
}
