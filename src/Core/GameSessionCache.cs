using System.Collections.Concurrent;

namespace tero.session.src.Core;

public class GameSessionCache<TSession>(ILogger<GameSessionCache<TSession>> logger)
{
    private readonly ConcurrentDictionary<string, CachedSession<TSession>> _cache = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];

    public ConcurrentDictionary<string, CachedSession<TSession>> GetCopy() => new(_cache);

    public async Task<Result<Error>> Insert(string key, TSession session)
    {
        try
        {
            var entry = new CachedSession<TSession>(session);
            if(!_cache.TryAdd(key, entry))
            {
                return Error.KeyExists;
            }

            return Result<Error>.Ok;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to insert into session cache");
            return Error.System;
        }
    }

    public async Task<Result<TSession, Error>> Upsert(string key, Func<TSession, Result<TSession, Error>> func)
    {
        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();

        try
        {
            if(!_cache.TryGetValue(key, out var entry))
            {
                return Error.GameNotFound;
            }

            var session = entry.GetSession();
            var result = func(session);
            entry.SetSession(session);
            
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

    public async Task<Result<TSession, Error>> Upsert(string key, Func<TSession, TSession> func)
    {
        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();

        try
        {
            if(!_cache.TryGetValue(key, out var entry))
            {
                return Error.GameNotFound;
            }

            var session = entry.GetSession();
            var result = func(session);
            entry.SetSession(session);

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

    public async Task<Result<TResult, Error>> Upsert<TResult>(string key, Func<TSession, TResult> func)
    {
        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();

        try
        {
            if (!_cache.TryGetValue(key, out var entry))
            {
                return Error.GameNotFound;
            }

            var session = entry.GetSession();
            var result = func(session);
            entry.SetSession(session);

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
            if(!_cache.TryRemove(key, out _))
            {
                logger.LogWarning("Tried removing non exising session from the cache");
            }

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