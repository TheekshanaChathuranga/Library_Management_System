using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserIdentityService.Api.Data;
using UserIdentityService.Api.Dtos.Roles;
using UserIdentityService.Api.Entities;
using UserIdentityService.Api.Services.Caching;

namespace UserIdentityService.Api.Services.Permissions;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IPermissionCacheService permissionCacheService) : IRoleService
{
    public async Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleManager.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleResponse
        {
            Id = r.Id,
            Name = r.Name!,
            Description = r.Description,
            Permissions = r.RolePermissions.Select(rp => rp.Permission.Code).ToList()
        });
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (await roleManager.RoleExistsAsync(request.Name))
        {
            throw new InvalidOperationException("Role already exists");
        }

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            NormalizedName = request.Name.ToUpperInvariant(),
            Description = request.Description
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        await UpsertRolePermissionsAsync(role, request.Permissions, cancellationToken);

        return await MapRoleAsync(role.Id, cancellationToken);
    }

    public async Task<RoleResponse> UpdatePermissionsAsync(Guid roleId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString()) ?? throw new KeyNotFoundException("Role not found");

        await UpsertRolePermissionsAsync(role, request.Permissions, cancellationToken);

        await InvalidateUsersForRoleAsync(role.Name!, cancellationToken);

        return await MapRoleAsync(roleId, cancellationToken);
    }

    private async Task<RoleResponse> MapRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await roleManager.Roles.FirstAsync(r => r.Id == roleId, cancellationToken);
        var permissions = await dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Code)
            .ToListAsync(cancellationToken);

        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            Permissions = permissions
        };
    }

    private async Task UpsertRolePermissionsAsync(ApplicationRole role, IEnumerable<string> permissions, CancellationToken cancellationToken)
    {
        var permissionCodes = permissions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (permissionCodes.Length == 0)
        {
            dbContext.RolePermissions.RemoveRange(dbContext.RolePermissions.Where(rp => rp.RoleId == role.Id));
        }
        else
        {
            var existingPermissions = await dbContext.Permissions
                .Where(p => permissionCodes.Contains(p.Code))
                .ToListAsync(cancellationToken);

            var missingCodes = permissionCodes.Except(existingPermissions.Select(p => p.Code), StringComparer.OrdinalIgnoreCase)
                .Select(code => new Permission { Id = Guid.NewGuid(), Code = code, Description = code })
                .ToArray();

            if (missingCodes.Length > 0)
            {
                await dbContext.Permissions.AddRangeAsync(missingCodes, cancellationToken);
                existingPermissions.AddRange(missingCodes);
            }

            dbContext.RolePermissions.RemoveRange(dbContext.RolePermissions.Where(rp => rp.RoleId == role.Id));

            var newLinks = existingPermissions
                .Select(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id })
                .ToArray();

            await dbContext.RolePermissions.AddRangeAsync(newLinks, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task InvalidateUsersForRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var users = await userManager.GetUsersInRoleAsync(roleName);
        foreach (var user in users)
        {
            await permissionCacheService.InvalidateAsync(user.Id, cancellationToken);
        }
    }
}
