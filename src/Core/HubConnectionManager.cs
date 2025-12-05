using System.Collections.Concurrent;
using tero.session.src.Features.Platform;

namespace tero.session.src.Core;

public class HubConnectionManager<T>(ILogger<HubConnectionManager<T>> logger, CacheTTLOptions options, PlatformClient platformClient)
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(options.ManagerMinuttes);
    private readonly ConcurrentDictionary<string, HubInfo> _manager = [];

    public ConcurrentDictionary<string, HubInfo> GetCopy() => new(_manager);

    public int Size() => _manager.Count;

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
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("Get")
                .WithDescription("Get on HubConnectionManager threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(Get));
            return Error.System;
        }
    }

    public Result<Error> Insert(string connectionId, HubInfo value)
    {
        try
        {
            value.SetTtl(_ttl);
            var added = _manager.TryAdd(connectionId, value);
            if (!added)
            {
                return Error.KeyExists;
            }

            return Result<Error>.Ok;
        }
        catch (OverflowException error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Create)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("Insert")
                .WithDescription("HubConnectionManager overflow on insert")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, "Insert - Cache overflow");
            return Error.Overflow;
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Create)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("Insert")
                .WithDescription("Insert into HubConnectionManager threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
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
            var log = LogBuilder.New()
                .WithAction(LogAction.Delete)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("Remove")
                .WithDescription("Remove from HubConnectionManager threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(Remove));
            return Error.System;
        }
    }
}