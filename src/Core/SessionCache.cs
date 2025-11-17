using Microsoft.Extensions.Caching.Hybrid;

namespace tero_session.src.Core;

public class SessionCache(HybridCache cache, ILogger<SessionCache> logger)
{
    public async Task<Result<T?, Exception>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await cache.GetOrCreateAsync<T>(
                key,
                async _ => default(T)!,
                cancellationToken: cancellationToken
            );
            
            return result;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error getting cache entry for key: {Key}", key);
            return error;
        }
    }

    public async Task<Result<bool, Exception>> SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.SetAsync(key, value, cancellationToken: cancellationToken);
            return true;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error setting cache entry for key: {Key}", key);
            return error;
        }
    }
}
