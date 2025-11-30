using System.Collections.Concurrent;

namespace tero.session.src.Core;

public class HubConnectionManager<T>(ILogger<HubConnectionManager<T>> logger, CacheTTLOptions options)
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(options.ManagerMinuttes);
    private readonly ConcurrentDictionary<string, HubInfo> _manager = [];

    public ConcurrentDictionary<string, HubInfo> GetCopy() => new(_manager);

    public Result<Option<HubInfo>, Error> Get(string connectionId)
    {
        try
        {
            if (connectionId == string.Empty || connectionId is null)
            {
                return Error.NullReference;
            }

            if (!_manager.TryGetValue(connectionId, out var value))
            {
                return Option<HubInfo>.None;
            }

            if (value is null)
            {
                return Option<HubInfo>.None;
            }

            return Option<HubInfo>.Some(value);
        }
        catch (Exception error)
        {
            logger.LogError(error, nameof(Get));
            return Error.System;
        }
    }

    public Result<bool, Error> Insert(string connectionId, HubInfo value)
    {
        try
        {
            value.SetTtl(_ttl);
            return _manager.TryAdd(connectionId, value);
        }
        catch (OverflowException error)
        {
            // SYSTLOG
            logger.LogError(error, "Insert - Cache overflow");
            return Error.Overflow;
        }
        catch (Exception error)
        {
            logger.LogError(error, nameof(Insert));
            return Error.System;
        }
    }

    public Result<Option<HubInfo>, Error> Remove(string connectionId)
    {
        try
        {

            if (connectionId == string.Empty || connectionId is null)
            {
                return Error.NullReference;
            }

            if (!_manager.TryRemove(connectionId, out var value))
            {
                return Option<HubInfo>.None;
            }

            if (value is null)
            {
                return Option<HubInfo>.None;
            }

            return Option<HubInfo>.Some(value);
        }
        catch (Exception error)
        {
            logger.LogError(error, nameof(Remove));
            return Error.System;
        }
    }
}