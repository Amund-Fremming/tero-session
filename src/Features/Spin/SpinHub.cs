using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.SignalR;
using tero.session.src.Core;

namespace tero.session.src.Features.Spin;

public class SpinHub(ILogger<SpinHub> logger, HubConnectionManager<SpinSession> manager, GameSessionCache<SpinSession> cache) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        logger.LogDebug("Client connected to SpinSession");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var option = manager.Get(Context.ConnectionId);
        if (option.IsNone())
        {
            // TODO - system log
            logger.LogError("Failed to get diconnecting users data to gracefully remove");
            await base.OnDisconnectedAsync(exception);
            return;
        }

        var hubInfo = option.Unwrap();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, hubInfo.GameKey);

        var result = await cache.Upsert(
            hubInfo.GameKey,
            session => session.RemoveUser(hubInfo.UserId)
        );

        if (result.IsErr())
        {
            await CoreUtils.Broadcast(Clients, result.Err());
            return;
        }

        var removeOption = result.Unwrap();
        if (removeOption.IsSome())
        {
            var newHostId = removeOption.Unwrap();
            await Clients.GroupExcept(hubInfo.GameKey, Context.ConnectionId).SendAsync("host", newHostId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddUser(string key, Guid userId)
    {
        var result = await cache.Upsert(
            key,
            session => session.AddUser(userId)
        );

        if (result.IsErr())
        {
            await CoreUtils.Broadcast(Clients, result.Err());
            return;
        }

        var option = result.Unwrap();
        if (option.IsSome())
        {
            var newHost = option.Unwrap();
            await Clients.Group(key).SendAsync("host", newHost);
        }

        var success = manager.Insert(Context.ConnectionId, new HubInfo(key, userId));
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
        var result = await cache.Upsert(
            key,
            session => session.AddRound(round)
        );

        if (result.IsErr())
        {
            await CoreUtils.Broadcast(Clients, result.Err());
            return;
        }

        var session = result.Unwrap();
        await Clients.Group(key).SendAsync("iterations", session.Iterations);
        logger.LogDebug("User added a round to SpinSession");
    }

    public async Task StartGame(string key)
    {
        var result = await cache.Upsert<SpinSession>(
            key,
            session => session.Start()
        );

        if (result.IsErr())
        {
            await CoreUtils.Broadcast(Clients, result.Err());
            return;
        }

        var round = result.Unwrap();
        await Clients.Caller.SendAsync("round", round);
        // TODO - persist to platform
    }
    public async Task StartRound(string key)
    {
        var result = await cache.Upsert<SpinSession>(key, session => session.Start());
        if (result.IsErr())
        {
            await CoreUtils.Broadcast(Clients, result.Err());
            return;
        }

        var session = result.Unwrap();

        var userIds = session.GetUserIds();
        // TODO - this does not increment players chosen in the cache
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
        var result = await cache.Upsert(
            key,
            session => session.IncrementRound()
        );

        if (result.IsErr())
        {
            await CoreUtils.Broadcast(Clients, result.Err());
            return;
        }

        var updatedSession = result.Unwrap();
        var round = updatedSession.GetRoundText();

        await Clients.Caller.SendAsync("round", round);
        await Clients.Group(key).SendAsync("state", updatedSession.State);

        logger.LogDebug("SpinSession round initialized");
    }
}