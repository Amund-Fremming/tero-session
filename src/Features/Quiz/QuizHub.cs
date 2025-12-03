using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.SignalR;
using tero.session.src.Core;
using tero.session.src.Features.Platform;

namespace tero.session.src.Features.Quiz;

public class QuizHub(GameSessionCache<QuizSession> cache, HubConnectionManager<QuizSession> manager, ILogger<QuizHub> logger, PlatformClient platformClient) : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            await base.OnConnectedAsync();
            logger.LogDebug("Client connected to QuizSession");

            // TODO - remove
            platformClient.GetType();
        }
        catch (Exception error)
        {
            await platformClient.LogToBackend(error, LogCeverity.Warning);
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
                await platformClient.LogToBackend(
                    new Exception("Failed to get disconnecting users data to gracefully remove"),
                    LogCeverity.Warning
                );
                logger.LogError("Failed to get diconnecting users data to gracefully remove");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            var hubInfo = option.Unwrap();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, hubInfo.GameKey);
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception error)
        {
            await platformClient.LogToBackend(error, LogCeverity.Warning);
            logger.LogError(error, nameof(OnConnectedAsync));
        }
    }

    public async Task AddQuestion(string key, string question)
    {
        try
        {
            var result = await cache.Upsert<QuizSession>(key, session => session.AddQuesiton(question));
            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger);
                return;
            }

            logger.LogDebug("Added question to QuizSession");

        }
        catch (Exception error)
        {
            await platformClient.LogToBackend(error, LogCeverity.Warning);
            logger.LogError(error, nameof(OnConnectedAsync));
        }
    }

    public async Task StartGame(string key)
    {
        try
        {
            var result = await cache.Upsert<QuizSession>(key, session => session.Start());
            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger);
                return;
            }

            var game = result.Unwrap();
            await Clients.Caller.SendAsync("game", game);

            var removeResult = await cache.Remove(key);
            if (removeResult.IsErr())
            {
                // log?
                logger.LogError("Failed to remove game");
                await CoreUtils.Broadcast(Clients, removeResult.Err(), logger);
                return;
            }

            // TODO - persist game to platform
        }
        catch (Exception error)
        {
            await platformClient.LogToBackend(error, LogCeverity.Critical);
            logger.LogError(error, nameof(OnConnectedAsync));
        }
    }
}