using Microsoft.AspNetCore.SignalR;
using tero.session.src.Core;

namespace tero.session.src.Features.Quiz;

public class QuizHub(GameSessionCache cache, HubConnectionCache<QuizSession> connectionMap, ILogger<QuizHub> logger, PlatformClient platformClient) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        logger.LogDebug("Client connected to QuizSession");
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