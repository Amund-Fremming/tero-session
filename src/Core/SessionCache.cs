using Microsoft.Extensions.Caching.Hybrid;

namespace tero_session.src.Core;

public class SessionCache(HybridCache cache, ILogger<SessionCache> logger)
{
    public async Task<Result<T?, Exception>> Get<T>(string key)
    {
        try
        {
            var value = await cache.GetOrCreateAsync<T?>(
                key,
                cancel => ValueTask.FromResult(default(T?))
            );
            
            return Result<T?, Exception>.Ok(value);
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error getting value from cache with key: {Key}", key);
            return Result<T?, Exception>.Err(error);
        }
    }

    public async Task<Result<bool, Exception>> Insert<T>(string key, T value)
    {
        try
        {
            await cache.SetAsync(key, value);
            return Result<bool, Exception>.Ok(true);
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error inserting value to cache with key: {Key}", key);
            return Result<bool, Exception>.Err(error);
        }
    }
}
