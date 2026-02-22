using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserIdentityService.Api.Dtos.Users;
using UserIdentityService.Api.Entities;
using UserIdentityService.Api.Services.Caching;

namespace UserIdentityService.Api.Services.Auth;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IPermissionCacheService permissionCacheService) : IUserService
{
    public async Task<IEnumerable<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users.ToListAsync(cancellationToken);
        var results = new List<UserSummary>(users.Count);

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            results.Add(new UserSummary
            {
                Id = user.Id,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                Roles = roles
            });
        }

        return results;
    }

    public async Task<UserSummary?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new UserSummary
        {
            Id = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            Roles = roles
        };
    }

    public async Task<UserSummary> AssignRolesAsync(Guid userId, AssignRolesRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new KeyNotFoundException("User not found");

        var targetRoles = request.Roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        foreach (var role in targetRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                throw new InvalidOperationException($"Role '{role}' does not exist");
            }
        }

        var existingRoles = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(user, existingRoles);
        if (!removeResult.Succeeded)
        {
            ThrowIdentityException(removeResult);
        }

        var addResult = await userManager.AddToRolesAsync(user, targetRoles);
        if (!addResult.Succeeded)
        {
            ThrowIdentityException(addResult);
        }

        await permissionCacheService.InvalidateAsync(user.Id, cancellationToken);

        return new UserSummary
        {
            Id = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            Roles = targetRoles
        };
    }

    private static void ThrowIdentityException(IdentityResult result)
    {
        var errors = string.Join(",", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException(errors);
    }
}
