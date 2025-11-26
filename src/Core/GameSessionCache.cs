using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Hybrid;

namespace tero.session.src.Core;

public class GameSessionCache(ILogger<GameSessionCache> logger, HybridCache cache)
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];

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

    public async Task<Result<TSession, Error>> Upsert<TSession>(string key, Func<TSession, Result<TSession, Error>> func)
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
                return Error.GameNotFound;
            }

            var result = func(session);
            await cache.SetAsync(key, session);
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to upsert into session cache");
            return Error.System;
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task<Result<TSession, Error>> Upsert<TSession>(string key, Func<TSession, TSession> func)
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
                return Error.GameNotFound;
            }

            var result = func(session);
            await cache.SetAsync(key, session);
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to upsert into session cache");
            return Error.System;
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task<Result<TResult, Error>> Upsert<TSession, TResult>(string key, Func<TSession, TResult> func)
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
                return Error.GameNotFound;
            }

            var result = func(session);
            await cache.SetAsync(key, session);
            return Result<TResult, Error>.Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to upsert into session cache");
            return Error.System;
        }
        finally
        {
            sem.Release();
        }
    }

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
}