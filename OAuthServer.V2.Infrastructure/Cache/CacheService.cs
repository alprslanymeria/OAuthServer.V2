using Microsoft.Extensions.Caching.Distributed;
using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.Infrastructure.Cache;

public class CacheService(
    
    IDistributedCache distributedCache
    
    ) : ICacheService
{
    private readonly IDistributedCache _distributedCache = distributedCache;

    public async Task SetStringAsync(string key, string value, TimeSpan? absoluteExpiration = null)
    {
        var options = new DistributedCacheEntryOptions();

        if (absoluteExpiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = absoluteExpiration;

        await _distributedCache.SetStringAsync(key, value, options);
    }

    public Task<string?> GetStringAsync(string key)
        => _distributedCache.GetStringAsync(key);

    public Task RemoveAsync(string key)
        => _distributedCache.RemoveAsync(key);
}
