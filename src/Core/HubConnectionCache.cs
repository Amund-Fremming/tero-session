using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace tero.session.src.Core;

public class HubConnectionCache<T>
{
    private readonly ConcurrentDictionary<string, HubInfo> _map = [];

    public Option<HubInfo> Get(string connectionId)
    {
        if (!_map.TryGetValue(connectionId, out var value))
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
        => _map.TryAdd(connectionId, value);

    public Option<HubInfo> Remove(string connectionId)
    {
        if (!_map.TryRemove(connectionId, out var value))
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