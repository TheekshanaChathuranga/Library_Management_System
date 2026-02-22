using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace UserIdentityService.Api.Services.Caching;

public class PermissionCacheService(IDistributedCache cache, ILogger<PermissionCacheService> logger) : IPermissionCacheService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public async Task CachePermissionsAsync(Guid userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(permissions.Distinct().ToArray());
        await cache.SetStringAsync(CacheKey(userId), payload, CacheOptions, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>?> GetCachedPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(CacheKey(userId), cancellationToken);
        if (string.IsNullOrEmpty(payload))
        {
            return null;
        }

        try
        {
            var permissions = JsonSerializer.Deserialize<string[]>(payload);
            return permissions ?? Array.Empty<string>();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize permissions cache for user {UserId}", userId);
            await cache.RemoveAsync(CacheKey(userId), cancellationToken);
            return null;
        }
    }

    public Task InvalidateAsync(Guid userId, CancellationToken cancellationToken = default)
        => cache.RemoveAsync(CacheKey(userId), cancellationToken);

    private static string CacheKey(Guid userId) => $"perm:{userId}";
}
