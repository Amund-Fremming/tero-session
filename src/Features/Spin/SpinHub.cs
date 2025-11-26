using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;
using tero.session.src.Core;
using tero.session.src.Core.Spin;

namespace tero.session.src.Features.Spin;

// Solution to better reading:
// More effective to just make all upserts return a readonly copy of the object so i can broadcast whatever
// Sessions might need some more functionslity to get this, and make operations only return error or ok not acual data
// Simplifies Upsert alot also

public class SpinHub(ILogger<SpinHub> logger, HubConnectionCache connectionMap, GameSessionCache cache) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        logger.LogDebug("Client connected to SpinSession");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var option = connectionMap.Get(Context.ConnectionId);
        if (option.IsNone())
        {
            // TODO - system log
            logger.LogError("Failed to get diconnecting users data to gracefully remove");
            await base.OnDisconnectedAsync(exception);
            return;
        }

        var hubInfo = option.Unwrap();
        var result = await cache.Upsert<SpinSession, Option<Guid>>(hubInfo.GameKey, session =>
        {
            return session.RemoveUser(hubInfo.UserId);
        });

        if (result.IsErr())
        {
            logger.LogCritical("Requested SpinSession does not exist");
            await base.OnDisconnectedAsync(exception);
            return;
        }

        var removeOption = result.Unwrap();
        if (removeOption.IsSome())
        {
            var newHost = option.Unwrap();
            await Clients.GroupExcept(hubInfo.GameKey, Context.ConnectionId).SendAsync("host", newHost);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddUser(string key, Guid userId)
    {
        var result = await cache.Upsert<SpinSession, Option<Guid>>(key, session =>
        {
            return session.AddUser(userId);
        });

        if (result.IsErr())
        {
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var option = result.Unwrap();
        if (option.IsSome())
        {
            var newHost = option.Unwrap();
            await Clients.Group(key).SendAsync("host", newHost);
        }

        var success = connectionMap.Insert(Context.ConnectionId, new HubInfo(key, userId));
        if (!success)
        {
            // TODO - log system log
            logger.LogError("Failed to add connection id to connection cache");
            await Clients.Caller.SendAsync("error", "En feil har skjedd, forsøk igjen");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, key);
        logger.LogDebug("User added to SpinSession");
    }

    public async Task AddRound(string key, string round)
    {
        var result = await cache.Upsert<SpinSession, bool>(key, session =>
        {
            return session.AddRound(round);
        });

        if (result.IsErr())
        {
            logger.LogCritical("Requested SpinSession does not exist");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var success = result.Unwrap();
        if (!success)
        {
            await Clients.Caller.SendAsync("error", "Kan ikke legge til mer, spillet har startet");
            return;
        }

        var sessionReadOnly = await cache.Read<SpinSession>(key);
        if (sessionReadOnly.IsSome())
        {
            var session = sessionReadOnly.Unwrap();
            await Clients.Group(key).SendAsync("iterations", session.Iterations);
        }

        logger.LogDebug("User added a round to SpinSession");
    }

    public async Task StartGame(string key)
    {
        var result = await cache.Upsert<SpinSession, string>(key, session =>
        {
            return session.Start();
        });

        if (result.IsErr())
        {
            logger.LogCritical("Requested SpinSession does not exist");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var round = result.Unwrap();
        await Clients.Caller.SendAsync("round", round);
        // TODO - persist to platform
    }
    public async Task StartRound(string key)
    {
        var result = await cache.Upsert<SpinSession, string>(key, session =>
        {
            return session.Start();
        });

        if (result.IsErr())
        {
            logger.LogCritical("Requested SpinSession does not exist");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var round = result.Unwrap();

        var userIds = session.GetUserIds();
        var selected = session.GetSpinResult(2);

        var rng = new Random();
        int spinRounds = (int)(rng.NextDouble() * (6 * session.UsersCount()));

        // TODO - if 2 to be selected this loop should send out 2 selected, not 1 for each iteration
        for (var i = 0; i < spinRounds; i++)
        {
            foreach (var id in userIds)
            {
                await Clients.Group(key).SendAsync("selected", id);
            }
        }

        var tasks = new List<Task>();
        foreach (var id in selected)
        {
            var task = Clients.Group(key).SendAsync("selected", id);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        logger.LogDebug("Round players selected for SpinSession");
    }

    public async Task NextRound(string key)
    {
        var outerResult = await cache.Upsert<SpinSession, Result<string, SpinGameState>>(key, session =>
        {
            return session.NextRound();
        });

        if (outerResult.IsErr())
        {
            logger.LogCritical("Requested SpinSession does not exist");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var innerResult = outerResult.Unwrap();
        if (innerResult.IsErr())
        {
            var state = innerResult.Err();
            await Clients.Group(key).SendAsync("state", state);
            return;
        }

        var round = innerResult.Unwrap();
        await Clients.Caller.SendAsync("round", round);
        await Clients.Groups(key).SendAsync("state", state);

        logger.LogDebug("SpinSession round initialized");
    }


    public async Task StartGame(string key)
    {
        var result = await cache.GetOrErr<SpinSession>(key);
        if (result.IsErr())
        {
            logger.LogCritical(result.Err(), "Failed to get SpinSession");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var session = result.Unwrap();
        session.Start();
        var roundResult = session.NextRound();
        if (roundResult.IsErr())
        {
            logger.LogInformation("SpinSession is finished");
            await Clients.Group(key).SendAsync("state", roundResult.Err());
            return;
        }

        var round = roundResult.Unwrap();
        var state = session.State;

        await Clients.Groups(key).SendAsync("state", state);
        await Clients.Caller.SendAsync("round", round);
        logger.LogDebug("SpinSession round initialized");
    }
}