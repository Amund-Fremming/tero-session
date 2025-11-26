using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using tero.session.src.Core;
using tero.session.src.Features.Spin;

namespace tero.session.src.Features.Quiz;

/*
    TODO
    - update this to use map instead of getting data from the request
*/

public class QuizHub(GameSessionCache cache, ILogger<QuizHub> logger, PlatformClient platformClient) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        logger.LogDebug("Client connected to QuizSession");
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

    public async Task AddQuestion(string key, string question)
    {
        var success = await cache.Upsert<QuizSession>(key, session => session.AddQuesiton(question));
        if (!success)
        {
            logger.LogCritical("Requested QuizSession does not exist");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        logger.LogDebug("Added question to QuizSession");
    }

    public async Task StartGame(string key)
    {
        var option = await cache.Read<SpinSession>(key);
        if (option.IsNone())
        {
            logger.LogCritical("Requested QuizSession does not exist");
            await Clients.Caller.SendAsync("error", "Spillet finnes ikke");
            return;
        }

        var game = option.Unwrap();
        await Clients.Caller.SendAsync("game", game);

        var success = await cache.Remove(key);
        if (!success)
        {
            logger.LogError("Failed to remove game");
            // log?
        }

        // TODO - persist game to platform
    }
}