using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Hybrid;

namespace tero.session.src.Core;

public class GameSessionCache(ILogger<GameSessionCache> logger, HybridCache cache)
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];

    /// Used for inserting new entry. This will override existing entries.
    public async Task<bool> Insert<TSession, TResult>(string key, TSession session)
    {
        try
        {
            await cache.SetAsync(key, session);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to insert into session cache");
            return false;
        }
    }

    public async Task<bool> Upsert<TSession>(string key, Action<TSession> func)
    {
        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();

        try
        {
            var session = await cache.GetOrCreateAsync(
                key,
                cancel => ValueTask.FromResult(default(TSession?))
            );

            if (session is null)
            {
                logger.LogError("Tried upsering non exising key for session");
                return false;
            }

            func(session);
            await cache.SetAsync(key, session);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to upsert into session cache");
            return false;
        }
        finally
        {
            sem.Release();
        }
    }

    /// Used updating entries
    public async Task<Result<TResult, Exception>> Upsert<TSession, TResult>(string key, Func<TSession, TResult> func)
    {
        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();

        try
        {
            var session = await cache.GetOrCreateAsync(
                key,
                cancel => ValueTask.FromResult(default(TSession?))
            );

            if (session is null)
            {
                logger.LogError("Tried upsering non exising key for session");
                return new Exception("Session does not exist for requesting key");
            }

            var result = func(session);
            await cache.SetAsync(key, session);
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to upsert into session cache");
            return e;
        }
        finally
        {
            sem.Release();
        }
    }

    /// Used for removing entries
    public async Task<bool> Remove(string key)
    {
        try
        {
            await cache.RemoveAsync(key);
            if (_locks.Remove(key, out var sem))
            {
                sem.Dispose();
            }

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove session from cache");
            return false;
        }
    }

    /// Should only be used for read only operations
    public async Task<Option<TSession>> Read<TSession>(string key)
    {
        try
        {
            var session = await cache.GetOrCreateAsync(
                key,
                cancel => ValueTask.FromResult(default(TSession?))
            );

            if (session is null)
            {
                return Option<TSession>.None;
            }

            return Option<TSession>.Some(session);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove session from cache");
            return Option<TSession>.None;
        }
    }
}