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
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName(nameof(OnConnectedAsync))
                .WithDescription("QuizHub OnConnectedAsync failed")
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
                    .WithFunctionName(nameof(OnDisconnectedAsync))
                    .WithDescription("Failed to get disconnecting user's data to gracefully remove")
                    .Build();

                platformClient.CreateSystemLogAsync(log);
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
            var log = LogBuilder.New()
                .WithAction(LogAction.Delete)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName(nameof(OnDisconnectedAsync))
                .WithDescription("QuizHub OnDisconnectedAsync threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(OnDisconnectedAsync));
        }
    }

    public async Task AddQuestion(string key, string question)
    {
        try
        {
            var result = await cache.Upsert<QuizSession>(key, session => session.AddQuesiton(question));
            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger, platformClient);
                return;
            }

            logger.LogDebug("Added question to QuizSession");

        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Create)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName(nameof(AddQuestion))
                .WithDescription("Add Question to QuizSession threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(AddQuestion));
        }
    }

    public async Task StartGame(string key)
    {
        try
        {
            var result = await cache.Upsert<QuizSession>(key, session => session.Start());
            if (result.IsErr())
            {
                await CoreUtils.Broadcast(Clients, result.Err(), logger, platformClient);
                return;
            }

            var game = result.Unwrap();
            await Clients.Caller.SendAsync("game", game);

            var removeResult = await cache.Remove(key);
            if (removeResult.IsErr())
            {
                // log?
                logger.LogError("Failed to remove game");
                await CoreUtils.Broadcast(Clients, removeResult.Err(), logger, platformClient);
                return;
            }

            var persistResult = await platformClient.PersistGame();
            if(persistResult.IsErr())
            {

            }
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Update)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName(nameof(StartGame))
                .WithDescription("Start QuizSession threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(StartGame));
        }
    }
}