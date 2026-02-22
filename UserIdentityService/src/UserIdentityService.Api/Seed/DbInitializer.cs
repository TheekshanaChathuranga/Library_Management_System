using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserIdentityService.Api.Configuration;
using UserIdentityService.Api.Constants;
using UserIdentityService.Api.Data;
using UserIdentityService.Api.Entities;

namespace UserIdentityService.Api.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        var context = scopedProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scopedProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminSeedOptions = scopedProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;

        await context.Database.MigrateAsync(cancellationToken);

        await EnsurePermissionsAsync(context, cancellationToken);
        await EnsureRolesAsync(roleManager, context, cancellationToken);
        await EnsureAdminUserAsync(userManager, roleManager, adminSeedOptions, cancellationToken);
    }

    private static async Task EnsurePermissionsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        foreach (var code in Permissions.Default)
        {
            if (!await context.Permissions.AnyAsync(p => p.Code == code, cancellationToken))
            {
                context.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Description = code
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureRolesAsync(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, CancellationToken cancellationToken)
    {
        await EnsureRoleWithPermissionsAsync(roleManager, context, SystemRoles.Admin, Permissions.Default, cancellationToken);
        await EnsureRoleWithPermissionsAsync(roleManager, context, SystemRoles.Librarian, new[]
        {
            Permissions.ManageCatalog,
            Permissions.ViewCatalog,
            Permissions.IssueLoans
        }, cancellationToken);
        await EnsureRoleWithPermissionsAsync(roleManager, context, SystemRoles.Member, new[]
        {
            Permissions.ViewCatalog
        }, cancellationToken);
    }

    private static async Task EnsureRoleWithPermissionsAsync(
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext context,
        string roleName,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = $"{roleName} role"
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }
        }

        await UpdateRolePermissionsAsync(context, role, permissions, cancellationToken);
    }

    private static async Task UpdateRolePermissionsAsync(ApplicationDbContext context, ApplicationRole role, IEnumerable<string> permissions, CancellationToken cancellationToken)
    {
        var permissionEntities = await context.Permissions
            .Where(p => permissions.Contains(p.Code))
            .ToListAsync(cancellationToken);

        context.RolePermissions.RemoveRange(context.RolePermissions.Where(rp => rp.RoleId == role.Id));

        var newLinks = permissionEntities.Select(p => new RolePermission
        {
            RoleId = role.Id,
            PermissionId = p.Id
        });

        await context.RolePermissions.AddRangeAsync(newLinks, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        AdminSeedOptions options,
        CancellationToken cancellationToken)
    {
        var admin = await userManager.FindByEmailAsync(options.Email);
        if (admin is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            UserName = options.Email,
            Email = options.Email,
            DisplayName = options.DisplayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, options.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        await userManager.AddToRoleAsync(user, SystemRoles.Admin);
    }
}
