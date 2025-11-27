using Microsoft.AspNetCore.SignalR;
using tero.session.src.Features.Quiz;
using tero.session.src.Features.Spin;

namespace tero.session.src.Core;

public class CacheCleanupJob(
    ILogger<CacheCleanupJob> logger,
    GameSessionCache cache,
    HubConnectionCache<SpinSession> spinMap,
    HubConnectionCache<QuizSession> quizMap,
    IHubContext<SpinHub> spinHub,
    IHubContext<QuizHub> quizHub
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Cache cleanup service started");

        // TODO - remove
        cache.GetType();
        spinMap.GetType();
        quizMap.GetType();
        spinHub.GetType();
        quizHub.GetType();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                logger.LogDebug("Running cache cleanup");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in cache cleanup");
            }
        }

        logger.LogInformation("Cache cleanup service stopped");
    }
}