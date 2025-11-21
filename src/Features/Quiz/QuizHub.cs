using Microsoft.AspNetCore.SignalR;
using tero.session.src.Core;

namespace tero.session.src.Features.Quiz;

public class QuizHub(GameSessionCache cache, ILogger<QuizHub> logger, PlatformClient platformClient) : Hub
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

        var result = await  cache.GetOrErr<QuizSession>(key);
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

    public async Task AddQuestion(string key, string question)
    {
        var result = await cache.GetOrErr<QuizSession>(key);
        if (result.IsErr())
        {
            // sys log
            logger.LogWarning("Failed to get quiz session from cache");
            await  Clients.Caller.SendAsync("error", "Spillet finnes ikke lenger");
        }
        
        var session = result.Unwrap();
        session.AddQuesiton(question);

        await Clients.Group(key).SendAsync("iterations", session.Iterations);
        var updateResult = await cache.Update(key, session);
        if (updateResult.IsErr())
        {
            // TODO - add syslog
            logger.LogError("Failed to update quiz session to cache");
        }
    }

    public async Task StartGame(string key)
    {
        // TODO - persist game to platform
        var result = await cache.GetOrErr<QuizSession>(key);
        if (result.IsErr())
        {
            // TODO - syslog
            logger.LogWarning("Failed to get quiz session from cache");
            await  Clients.Caller.SendAsync("error", "Spillet finnes ikke lenger");
        }

        var session = result.Unwrap();
        await Clients.Caller.SendAsync("game", session.Start());

        var releaseResult = await platformClient.FreeGameKey(key);
        if(releaseResult.IsErr())
        {
            // TODO - syslog
            logger.LogError("Failed to release game key");
        }
    }
}