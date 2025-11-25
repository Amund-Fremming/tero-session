using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.ObjectPool;
using tero.session.src.Core;

namespace tero.session.src.Features.Spin;

public class SpinHub(GameSessionCache cache, ILogger<SpinHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var query = Context.GetHttpContext()?.Request.Query;
        if (query is null)
        {
            logger.LogWarning("Failed to read query from http context on incomming request");
            await Clients.Caller.SendAsync("error", "Klarte ikke bli koble til spillet");
            return;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var query = Context.GetHttpContext()?.Request.Query;
        if (query is null)
        {
            logger.LogWarning("Failed to read query from http context on incomming request");
            await Clients.Caller.SendAsync("error", "Klarte ikke koble til spillet");
            return;
        }

        // TODO - Game might need a lock when accessed
        // remove user from game
        // may need a disconnect map for mapping conection id to guid, or store it together with the normal id
        // does also need to have game_key, so maybe when a user joins a game, add it in a user cache or something here
        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddUser(string key, Guid userId)
    {
        var result = await cache.GetOrErr<SpinSession>(key);
        if(result.IsErr())
        {
            logger.LogCritical(result.Err(), "Failed to get session");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var session = result.Unwrap();
        var option = session.AddUser(userId);
        if(option.IsSome())
        {
            logger.LogInformation("New host set for SpinSession");
            await Clients.Caller.SendAsync("host", option.Data);
        }

        await cache.Update(key, session);
        await Groups.AddToGroupAsync(Context.ConnectionId, key);
        await Clients.Caller.SendAsync("iterations", session.Iterations);

        logger.LogDebug("User added to SpinSession");
    }

    public async Task AddRound(string key, string round)
    {
        var result = await cache.GetOrErr<SpinSession>(key);
        if(result.IsErr())
        {
            logger.LogCritical(result.Err(), "Failed to get SpinSession");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var session = result.Unwrap();
        var success = session.AddRound(round);
        if (!success)
        {
            logger.LogInformation("User failed to add a round, SpinSession closed");
            await Clients.Caller.SendAsync("error", "Kan ikke legge til fler, spillet har startet");
            return;
        }

        await cache.Update(key, session);
        await Clients.Group(key).SendAsync("iterations", session.Iterations);
        logger.LogDebug("User added a round to SpinSession");
    }

    public async Task StartRound(string key)
    {
        var result = await cache.GetOrErr<SpinSession>(key);
        if(result.IsErr())
        {
            logger.LogCritical(result.Err(), "Failed to get SpinSession");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var session = result.Unwrap();
        var userIds = session.GetUserIds();
        var selected = session.GetSpinResult(2);

        var rng = new Random();
        int spinRounds = (int)(rng.NextDouble() * (6*session.UsersCount()));

        for (var i = 0; i < spinRounds; i++)
        {
            foreach(var id in userIds)
            {
                await Clients.Group(key).SendAsync("selected", id);
            }
        }

        var tasks = new List<Task>();
        foreach(var id in selected)
        {
            var task = Clients.Group(key).SendAsync("selected", id);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        logger.LogDebug("Round players selected for SpinSession");
    }

    public async Task NextRound(string key)
    {        
        var result = await cache.GetOrErr<SpinSession>(key);
        if(result.IsErr())
        {
            logger.LogCritical(result.Err(), "Failed to get SpinSession");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var session = result.Unwrap();
        var round = session.NextRound();
        var state = session.State;

        await Clients.Groups(key).SendAsync("state", state);
        await Clients.Caller.SendAsync("round", round);
        logger.LogDebug("SpinSession round initialized");
    }


    public async Task StartGame(string key)
    {
        var result = await cache.GetOrErr<SpinSession>(key);
        if(result.IsErr())
        {
            logger.LogCritical(result.Err(), "Failed to get SpinSession");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var session = result.Unwrap();
        session.Start();
        var roundResult = session.NextRound();
        if(roundResult.IsErr())
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