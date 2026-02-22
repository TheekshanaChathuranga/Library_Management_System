using UserIdentityService.Api.Dtos.Users;

namespace UserIdentityService.Api.Services.Auth;

public interface IUserService
{
    Task<IEnumerable<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserSummary?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSummary> AssignRolesAsync(Guid userId, AssignRolesRequest request, CancellationToken cancellationToken = default);
}
