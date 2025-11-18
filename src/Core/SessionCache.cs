using System.Text.Json;
using Microsoft.Extensions.Caching.Hybrid;

namespace tero_session.src.Core;

public class GameSessionCache(HybridCache cache, ILogger<GameSessionCache> logger)
{
    public async Task<Result<T, Exception>> GetOrErr<T>(string key)
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

    public async Task<Result<bool, Exception>> Insert<T>(string key, JsonElement value)
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

            var session = JsonSerializer.Deserialize<T>(value);
            if (session is null)
            {
                return new NullReferenceException($"Failed to deserialize {nameof(T)}");
            }

            await cache.SetAsync(key, session);
            return true;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error inserting value to cache with key: {Key}", key);
            return error;
        }
    }

    public async Task<Result<bool, Exception>> Update<T>(string key, T value)
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

    public async Task<Result<bool, Exception>> AddUserToSession<T>(string key, Guid userId) where T : IJoinableSession
    {
        try
        {
            var result = await GetOrErr<T>(key);
            if (result.IsErr())
            {
                return new Exception("Failed to get session");
            }

            var session = result.Unwrap();
            session.AddToSession(userId);

            await cache.SetAsync(key, session);
            return true;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error updating value in cache with key: {Key}", key);
            return error;
        }
    }
}
