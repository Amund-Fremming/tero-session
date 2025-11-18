using Microsoft.Extensions.Caching.Hybrid;

namespace tero_session.src.Core;



public class SessionCache<T>(HybridCache cache, ILogger<SessionCache<T>> logger) : ISessionCache<T> where T : class
{
    public async Task<Result<T, Exception>> Get(string key)
    {
        try
        {
            var value = await cache.GetOrCreateAsync(
                key,
                cancel => ValueTask.FromResult(default(T?))
            );

            if (value is null)
            {
                return new NullReferenceException("Session does not exist");
            }

            return value;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error getting value from cache with key: {Key}", key);
            return error;
        }
    }

    public async Task<Result<bool, Exception>> Insert(string key, T value)
    {
        try
        {
            var existing = await cache.GetOrCreateAsync(
                key,
                cancel => ValueTask.FromResult(default(T?))
            );

            if (existing != null)
            {
                logger.LogWarning("Key already exists");
                return false;
            }

            await cache.SetAsync(key, value);
            return true;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error inserting value to cache with key: {Key}", key);
            return error;
        }
    }

    public async Task<Result<bool, Exception>> Update(string key, T value)
    {
        try
        {
            await cache.SetAsync(key, value);
            return true;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error updating value in cache with key: {Key}", key);
            return error;
        }
    }
}
