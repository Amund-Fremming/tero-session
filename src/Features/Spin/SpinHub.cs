using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.ObjectPool;
using tero.session.src.Core;
using tero.session.src.Features.Platform;

namespace tero.session.src.Features.Spin;

public class SpinHub(ILogger<SpinHub> logger, HubConnectionManager<SpinSession> manager, GameSessionCache<SpinSession> cache, PlatformClient platformClient) : Hub
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
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("OnConnectedAsync")
                .WithDescription("SpinHub OnConnectedAsync threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
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
                await CoreUtils.Broadcast(Clients, result.Err(), logger, platformClient);
                return;
            }

            var option = result.Unwrap();
            if (option.IsNone())
            {
                var log = LogBuilder.New()
                    .WithAction(LogAction.Delete)
                    .WithCeverity(LogCeverity.Warning)
                    .WithFunctionName("OnDisconnectedAsync")
                    .WithDescription("Failed to get disconnecting user's data to gracefully remove")
                    .Build();

                platformClient.CreateSystemLogAsync(log);
                logger.LogError("Failed to get disconnecting user's data to gracefully remove");

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
                await CoreUtils.Broadcast(Clients, upsertResult.Err(), logger, platformClient);
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
            var log = LogBuilder.New()
                .WithAction(LogAction.Delete)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("OnDisconnectedAsync")
                .WithDescription("SpinHub OnDisconnectedAsync threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(OnDisconnectedAsync));
        }
    }

    public async Task ConnectToGroup(string key, Guid userId)
    {
        try
        {
            var result = await cache.Upsert(
                key,
                session => session.AddUser(userId)
            );

            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger, platformClient);
                return;
            }

            var session = result.Unwrap();
            if (session.IsHost(userId))
            {
                await Clients.Group(key).SendAsync("host", userId);
            }

            var insertResult = manager.Insert(Context.ConnectionId, new HubInfo(key, userId));
            if (insertResult.IsErr())
            {
                await CoreUtils.Broadcast(Clients, insertResult.Err(), logger, platformClient);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, key);
            logger.LogDebug("User added to SpinSession");

        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Create)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("AddUser")
                .WithDescription("add user to SpinSession threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(ConnectToGroup));
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
                await CoreUtils.Broadcast(Clients, result.Err(), logger, platformClient);
                return;
            }

            var session = result.Unwrap();
            await Clients.Group(key).SendAsync("iterations", session.Iterations);
            logger.LogDebug("User added a round to SpinSession");
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Create)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("AddRound")
                .WithDescription("Add round to SpinSession threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(AddRound));
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
                await CoreUtils.Broadcast(Clients, result.Err(), logger, platformClient);
                return;
            }

            var session = result.Unwrap();
            var roundText = session.GetRoundText();
            await Clients.Caller.SendAsync("round_text", roundText);
            await platformClient.PersistGame(GameType.Spin, key, session);
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Update)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("StartGame")
                .WithDescription("Start SpinSession threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(StartGame));
        }
    }
    public async Task StartRound(string key)
    {
        try
        {
            var result = await cache.Upsert(key, session => session.Start());
            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger, platformClient);
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
            var log = LogBuilder.New()
                .WithAction(LogAction.Update)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("StartRound")
                .WithDescription("Start SpinSession round threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(StartRound));
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
                await CoreUtils.Broadcast(Clients, result.Err(), logger, platformClient);
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
            var log = LogBuilder.New()
                .WithAction(LogAction.Update)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("NextRound")
                .WithDescription("Next SpinSession round threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(NextRound));
        }
    }
}