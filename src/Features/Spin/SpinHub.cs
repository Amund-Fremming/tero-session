using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.SignalR;
using tero.session.src.Core;

namespace tero.session.src.Features.Spin;

public class SpinHub(ILogger<SpinHub> logger, HubConnectionManager<SpinSession> manager, GameSessionCache<SpinSession> cache) : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            await base.OnConnectedAsync();
            logger.LogDebug("Client connected to SpinSession");
        }
        catch (Exception error)
        {
            // TODO - system log
            logger.LogError(error, nameof(OnConnectedAsync));
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var result = manager.Get(Context.ConnectionId);
            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger);
                return;
            }

            var option = result.Unwrap();
            if (option.IsNone())
            {
                // TODO - system log
                logger.LogError("Failed to get diconnecting users data to gracefully remove");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            var hubInfo = option.Unwrap();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, hubInfo.GameKey);

            var upsertResult = await cache.Upsert(
                hubInfo.GameKey,
                session => session.RemoveUser(hubInfo.UserId)
            );

            if (upsertResult.IsErr())
            {
                await CoreUtils.Broadcast(Clients, upsertResult.Err(), logger);
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
        catch (Exception error)
        {
            // TODO - system log
            logger.LogError(error, nameof(OnDisconnectedAsync));
        }
    }

    public async Task AddUser(string key, Guid userId)
    {
        try
        {
            var result = await cache.Upsert(
                key,
                session => session.AddUser(userId)
            );

            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger);
                return;
            }

            var option = result.Unwrap();
            if (option.IsSome())
            {
                var newHost = option.Unwrap();
                await Clients.Group(key).SendAsync("host", newHost);
            }

            var insertResult = manager.Insert(Context.ConnectionId, new HubInfo(key, userId));
            if (insertResult.IsErr())
            {
                await CoreUtils.Broadcast(Clients, insertResult.Err(), logger);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, key);
            logger.LogDebug("User added to SpinSession");

        }
        catch (Exception error)
        {
            // TODO - system log
            logger.LogError(error, nameof(OnDisconnectedAsync));
        }
    }

    public async Task AddRound(string key, string round)
    {
        try
        {
            var result = await cache.Upsert(
                key,
                session => session.AddRound(round)
            );

            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger);
                return;
            }

            var session = result.Unwrap();
            await Clients.Group(key).SendAsync("iterations", session.Iterations);
            logger.LogDebug("User added a round to SpinSession");
        }
        catch (Exception error)
        {
            // TODO - system log
            logger.LogError(error, nameof(OnConnectedAsync));
        }
    }

    public async Task StartGame(string key)
    {
        try
        {
            var result = await cache.Upsert(
                key,
                session => session.Start()
            );

            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger);
                return;
            }

            var round = result.Unwrap();
            await Clients.Caller.SendAsync("round", round);
            // TODO - persist to platform
        }
        catch (Exception error)
        {
            // TODO - system log
            logger.LogError(error, nameof(OnConnectedAsync));
        }
    }
    public async Task StartRound(string key)
    {
        try
        {
            var result = await cache.Upsert(key, session => session.Start());
            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger);
                return;
            }

            var session = result.Unwrap();
            var userIds = session.GetUserIds();
            const int selectedPerRound = 2;
            var selected = session.GetSpinResult(selectedPerRound);
            var rng = new Random();
            int spinRounds = rng.Next(2, 7);

            for (var i = 0; i < spinRounds; i++)
            {
                for (var j = 0; j < userIds.Count; j += selectedPerRound)
                {
                    var batch = userIds.Skip(j).Take(selectedPerRound);
                    foreach (var id in batch)
                    {
                        await Clients.Group(key).SendAsync("selected", id);
                    }

                    await Task.Delay(400);
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
        catch (Exception error)
        {
            // TODO - system log
            logger.LogError(error, nameof(OnConnectedAsync));
        }
    }

    public async Task NextRound(string key)
    {
        try
        {
            var result = await cache.Upsert(
                key,
                session => session.IncrementRound()
            );

            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger);
                return;
            }

            var updatedSession = result.Unwrap();
            var round = updatedSession.GetRoundText();

            await Clients.Caller.SendAsync("round", round);
            await Clients.Group(key).SendAsync("state", updatedSession.State);

            logger.LogDebug("SpinSession round initialized");
        }
        catch (Exception error)
        {
            // TODO - system log
            logger.LogError(error, nameof(OnConnectedAsync));
        }
    }
}