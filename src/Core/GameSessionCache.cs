using System.Collections.Concurrent;

namespace tero.session.src.Core;

public class GameSessionCache<TSession>(ILogger<GameSessionCache<TSession>> logger, CacheTTLOptions options)
{
    private readonly TimeSpan _ttl =  TimeSpan.FromMinutes(options.SessionMinuttes);
    private readonly ConcurrentDictionary<string, CachedSession<TSession>> _cache = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];

    public ConcurrentDictionary<string, CachedSession<TSession>> GetCopy() => new(_cache);

    public Result<Error> Insert(string key, TSession session)
    {
        try
        {
            var entry = new CachedSession<TSession>(session, _ttl);
            if (!_cache.TryAdd(key, entry))
            {
                return Error.KeyExists;
            }

            return Result<Error>.Ok;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Failed to insert into session cache");
            return Error.System;
        }
    }

    public async Task<Result<TSession, Error>> Upsert(string key, Func<TSession, Result<TSession, Error>> func)
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

            return result;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Failed to upsert into session cache");
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
        catch (Exception error)
        {
            logger.LogError(error, "Failed to upsert into session cache");
            return Error.System;
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task<bool> Remove(string key)
    {
        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();

        try
        {
            if (!_cache.TryRemove(key, out _))
            {
                logger.LogWarning("Tried removing non exising session from the cache");
            }

            return true;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Failed to remove session from cache");
            return false;
        }
        finally
        {
            sem.Release();

            if (_locks.Remove(key, out var removedSem))
            {
                removedSem.Dispose();
            }
        }
    }
}