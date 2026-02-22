namespace UserIdentityService.Api.Dtos.Users;

public class AssignRolesRequest
{
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}
