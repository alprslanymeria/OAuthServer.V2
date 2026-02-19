namespace OAuthServer.V2.Core.Services;

public interface ICacheService
{
    // THE METHODS IN THIS INTERFACE ARE IMPLEMENTED IN THE INFRASTRUCTURE LAYER.
    // PROVIDES AN ABSTRACTION OVER DISTRIBUTED CACHE TO AVOID DIRECT DEPENDENCY ON IDistributedCache.

    Task SetStringAsync(string key, string value, TimeSpan? absoluteExpiration = null);
    Task<string?> GetStringAsync(string key);
    Task RemoveAsync(string key);
}
