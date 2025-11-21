using Microsoft.AspNetCore.SignalR;
using tero.session.src.Core;

namespace tero.session.src.Features.Spin;

public class SpinHub(GameSessionCache cache, ILogger<SpinHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var query = Context.GetHttpContext()?.Request.Query;
        if (query is null)
        {
            await Clients.Caller.SendAsync("error", "Klarte ikke bli koble til spillet");
            return;
        }

        if (query["game_key"].FirstOrDefault() is not string key)
        {
            await Clients.Caller.SendAsync("error", "Spill id er ikke gyldig");
            return;
        }

        var result = await  cache.GetOrErr<SpinSession>(key);
        if (result.IsErr())
        {
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var session = result.Unwrap();

        await Groups.AddToGroupAsync(Context.ConnectionId, key);
        await Clients.Caller.SendAsync("iterations", session.Iterations);
        await base.OnConnectedAsync();

    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var query = Context.GetHttpContext()?.Request.Query;
        if (query is null)
        {
            await Clients.Caller.SendAsync("error", "Klarte ikke koble til spillet");
            return;
        }

        if (query["game_key"].FirstOrDefault() is not string key)
        {
            await Clients.Caller.SendAsync("error", "Spill id er ikke gyldig.");
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, key);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddUser(string key, Guid userId)
    {
        // TODO
    }

    public async Task AddRound(string key, string round)
    {
        // TODO
    }

    public async Task IncrementPlayersChosen(string key, string round)
    {
        // TODO
    }

    public IEnumerable<Guid> NextRound()
    {
        // TODO
        return null;
    }


    public async Task StartGame(string key)
    {
        // TODO
    }
}