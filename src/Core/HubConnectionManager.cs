using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace tero.session.src.Core;

public class HubConnectionManager<T>
{
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
        => _manager.TryAdd(connectionId, value);

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