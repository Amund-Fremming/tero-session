using System.Collections.Concurrent;

namespace tero.session.src.Core;

public class HubConnectionManager<T>(CacheTTLOptions options)
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(options.ManagerMinuttes);
    private readonly ConcurrentDictionary<string, HubInfo> _manager = [];

    public ConcurrentDictionary<string, HubInfo> GetCopy() => new(_manager);

    public Option<HubInfo> Get(string connectionId)
    {
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

    public bool Insert(string connectionId, HubInfo value)
    {
        value.SetTtl(_ttl);
        return _manager.TryAdd(connectionId, value);
    }

    public Option<HubInfo> Remove(string connectionId)
    {
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
}