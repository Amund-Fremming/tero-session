using Microsoft.AspNetCore.SignalR;
using tero.session.src.Core;
using tero.session.src.Features.Platform;

namespace tero.session.src.Features.Quiz;

public class QuizHub(GameSessionCache<QuizSession> cache, HubConnectionManager<QuizSession> manager, ILogger<QuizHub> logger, PlatformClient platformClient) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        logger.LogDebug("Client connected to QuizSession");

        // TODO - remove
        platformClient.GetType();
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

        // TODO - upadte new host

        var hubInfo = option.Unwrap();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, hubInfo.GameKey);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddQuestion(string key, string question)
    {
        var result = await cache.Upsert<QuizSession>(key, session => session.AddQuesiton(question));
        if (result.IsErr())
        {
            await CoreUtils.Broadcast(Clients, result.Err());
            return;
        }

        logger.LogDebug("Added question to QuizSession");
    }

    public async Task StartGame(string key)
    {
        var result = await cache.Upsert<QuizSession>(key, session => session.Start());
        if (result.IsErr())
        {
            await CoreUtils.Broadcast(Clients, result.Err());
            return;
        }

        var game = result.Unwrap();
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