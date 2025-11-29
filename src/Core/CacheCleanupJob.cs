using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.SignalR;
using tero.session.src.Features.Quiz;
using tero.session.src.Features.Spin;

namespace tero.session.src.Core;

public class CacheCleanupJob(
    ILogger<CacheCleanupJob> logger,
    GameSessionCache<SpinSession> spinCache,
    GameSessionCache<QuizSession> quizCache,
    HubConnectionManager<SpinSession> spinManager,
    HubConnectionManager<QuizSession> quizManager,
    IHubContext<SpinHub> spinHub,
    IHubContext<QuizHub> quizHub
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Cache cleanup service started");

        // TODO - remove
        spinCache.GetType();
        quizCache.GetType();
        spinManager.GetType();
        quizManager.GetType();
        spinHub.GetType();
        quizHub.GetType();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                logger.LogDebug("Running cache cleanup");

                _ = Task.Run(async () => await CleanupCache(spinHub, spinCache, CancellationToken.None));
                _ = Task.Run(async () => await CleanupCache(quizHub, quizCache, CancellationToken.None));

                _ = Task.Run(async () => await CleanupManager(spinHub, spinManager, spinCache));
                // TODO - make own cleanup for non player games to only remove from group
                //_ = Task.Run(async () => await CleanupManager(quizHub, quizManager));

            }
            catch (OperationCanceledException e)
            {
                logger.LogError(e, "Backgorund cleanup was cancelled");
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in cache cleanup");
            }
        }

        logger.LogInformation("Cache cleanup service stopped");
    }

    private async Task CleanupCache<TSession, THub>(IHubContext<THub> hub, GameSessionCache<TSession> cache, CancellationToken stoppingToken) where THub : Hub
    {
        foreach (var (key, value) in cache.GetCopy())
        {
            if (value.HasExpired())
            {
                var success = await cache.Remove(key);
                if (!success)
                {
                    // syslog?
                    logger.LogError("Background cleanup failed to remove entry from cache");
                }

                // TODO - make frontend call disconnect on this action
                // Fire and forget
                _ = hub.Clients.Groups(key).SendAsync("disconnect", "Spillet har blitt avsluttet");
            }
        }
    }

    private async Task CleanupManager<THub, TSession, TCleanup>(IHubContext<THub> hub,HubConnectionManager<TSession> manager, GameSessionCache<TCleanup> cache) where TCleanup : ICleanuppableSession<TSession> where THub : Hub
    {
        foreach (var (connId, info) in manager.GetCopy())
        {
            if (info.HasExpired())
            {
                // Fire and forget here?
                /////////
                var result = await cache.Upsert(info.GameKey, session => session.Cleanup(info.UserId));
                if (result.IsErr())
                {
                    // Syslog
                    logger.LogError($"Failed to cleanup session manager {result.Err()}");
                }

                await hub.Groups.RemoveFromGroupAsync(connId, info.GameKey);
                    ////
            }
        }
    }
}