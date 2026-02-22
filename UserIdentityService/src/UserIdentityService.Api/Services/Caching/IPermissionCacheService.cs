namespace UserIdentityService.Api.Services.Caching;

public interface IPermissionCacheService
{
    Task CachePermissionsAsync(Guid userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>?> GetCachedPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task InvalidateAsync(Guid userId, CancellationToken cancellationToken = default);
}
