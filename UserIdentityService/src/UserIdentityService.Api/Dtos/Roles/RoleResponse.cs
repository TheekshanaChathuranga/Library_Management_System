namespace UserIdentityService.Api.Dtos.Roles;

public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
}
