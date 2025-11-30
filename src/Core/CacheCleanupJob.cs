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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                logger.LogDebug("Running cache cleanup");

                _ = Task.Run(async () => await CleanupCache(spinHub, spinCache));
                _ = Task.Run(async () => await CleanupCache(quizHub, quizCache));

                _ = Task.Run(async () => await CleanupManager(spinHub, spinManager, spinCache));
                _ = Task.Run(() => CleanupManager(quizHub, quizManager));

            }
            catch (OperationCanceledException error)
            {
                logger.LogError(error, "Backgorund cleanup failed to cancel");
                break;
            }
            catch (Exception error)
            {
                // Syslog?
                logger.LogError(error, "CacheCleanupJob");
            }
        }

        logger.LogInformation("Cache cleanup service stopped");
    }

    private async Task CleanupCache<TSession, THub>(IHubContext<THub> hub, GameSessionCache<TSession> cache) where THub : Hub
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
                _ = hub.Clients.Groups(key).SendAsync("disconnect", "Spillet har blitt avsluttet");
            }
        }
    }

    private async Task CleanupManager<THub, TSession, TCleanup>(IHubContext<THub> hub, HubConnectionManager<TSession> manager, GameSessionCache<TCleanup> cache) where TCleanup : ICleanuppable<TSession> where THub : Hub
    {
        try
        {
        foreach (var (connId, info) in manager.GetCopy())
        {
            if (info.HasExpired())
            {
                // Fire and forget here?
                var result = await cache.Upsert(info.GameKey, session => session.Cleanup(info.UserId));
                if (result.IsErr())
                {
                    // Syslog
                    logger.LogError($"Failed to cleanup session manager {result.Err()}");
                }

                _ = hub.Groups.RemoveFromGroupAsync(connId, info.GameKey);
                    _ = manager.Remove(connId);
                }
            }
        }
        catch (Exception error)
        {
            // Systlog?
            logger.LogError(error, "CleanupManager: ICleanuppable");
        }
    }

    private void CleanupManager<THub, TSession>(IHubContext<THub> hub,HubConnectionManager<TSession> manager) where THub : Hub
    {
        try
        {
        foreach (var (connId, info) in manager.GetCopy())
        {
            if (info.HasExpired())
            {
                _ = hub.Groups.RemoveFromGroupAsync(connId, info.GameKey);
            }
        }
        }catch (Exception error)
        {
            logger.LogError(error, nameof(CleanupManager));
        }
    }
}