using UserIdentityService.Api.Dtos.Roles;

namespace UserIdentityService.Api.Services.Permissions;

public interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleResponse> UpdatePermissionsAsync(Guid roleId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default);
}
