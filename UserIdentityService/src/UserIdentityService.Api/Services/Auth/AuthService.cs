using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserIdentityService.Api.Constants;
using UserIdentityService.Api.Data;
using UserIdentityService.Api.Dtos.Auth;
using UserIdentityService.Api.Entities;
using UserIdentityService.Api.Services.Caching;

namespace UserIdentityService.Api.Services.Auth;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext dbContext,
    ITokenService tokenService,
    IPermissionCacheService permissionCache) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedRole = request.Role.Trim();
        var role = await roleManager.FindByNameAsync(normalizedRole) ?? throw new InvalidOperationException($"Role '{normalizedRole}' not found");

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("User already exists");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            MembershipId = request.MembershipId,
            LibrarianCode = request.LibrarianCode,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        await userManager.AddToRoleAsync(user, role.Name!);
        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshTokenValue, CancellationToken cancellationToken = default)
    {
        var token = await dbContext.RefreshTokens.Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue, cancellationToken);

        if (token is null || token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token invalid");
        }

        token.IsRevoked = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponseAsync(token.User, cancellationToken);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await userManager.GetRolesAsync(user);
        var cachedPermissions = await permissionCache.GetCachedPermissionsAsync(user.Id, cancellationToken);
        IReadOnlyCollection<string> permissions;

        if (cachedPermissions is not null)
        {
            permissions = cachedPermissions;
        }
        else
        {
            permissions = await LoadPermissionsForRolesAsync(roles, cancellationToken);
            await permissionCache.CachePermissionsAsync(user.Id, permissions, cancellationToken);
        }

        var (accessToken, expiresAt) = await tokenService.CreateAccessTokenAsync(user, roles, permissions);
        var refreshToken = tokenService.CreateRefreshToken(user);

        await PersistRefreshTokenAsync(refreshToken, user.Id, cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt,
            Roles = roles,
            Permissions = permissions
        };
    }

    private async Task PersistRefreshTokenAsync(RefreshToken refreshToken, Guid userId, CancellationToken cancellationToken)
    {
        var existingTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt < DateTime.UtcNow.AddDays(-30))
            .ToListAsync(cancellationToken);

        if (existingTokens.Count > 0)
        {
            dbContext.RefreshTokens.RemoveRange(existingTokens);
        }

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<string>> LoadPermissionsForRolesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => new { r.Id })
            .ToListAsync(cancellationToken);

        if (roles.Count == 0)
        {
            return Array.Empty<string>();
        }

        var roleIds = roles.Select(r => r.Id).ToArray();

        var permissions = await dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
